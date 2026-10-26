using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Xunit;

namespace Glimmerfall.Sim.Tests;

/// <summary>
/// Тест-запрет (скилл sim-determinism): в Sim.dll нет float/double, тригонометрии Math/MathF, System.Random, системного
/// времени, потоков, обхода хеш-таблиц и ссылок на движок. Проверяется IL собранной сборки — сигнатуры полей, методов,
/// локальных переменных и опкоды, — а не grep по исходникам: обещание в документе без теста не работает (урок aegis-draft).
/// </summary>
public class SimPurityTests
{
    private static readonly string[] BannedTypes =
    {
        "System.Single", "System.Double", "System.Decimal", "System.Math", "System.MathF", "System.Random",
        "System.DateTime", "System.DateTimeOffset", "System.TimeSpan", "System.Environment", "System.Guid", "System.HashCode",
        "System.Diagnostics.Stopwatch", "System.Threading.Thread", "System.Threading.ThreadPool", "System.Threading.Timer",
        "System.Threading.Tasks.Task", "System.Threading.Tasks.Parallel",
        "System.Collections.Hashtable",
        "System.Collections.Generic.Dictionary`2/Enumerator", "System.Collections.Generic.Dictionary`2/KeyCollection",
        "System.Collections.Generic.Dictionary`2/ValueCollection", "System.Collections.Generic.HashSet`1/Enumerator",
    };

    private static readonly string[] BannedNamespacePrefixes =
    {
        "Godot", "UnityEngine", "System.Numerics", "System.Runtime.Intrinsics", "System.Security.Cryptography",
    };

    /// <summary>Хеш строки рандомизирован на процесс. Object::GetHashCode не запрещён: его вызывают переопределения у структур.</summary>
    private static readonly string[] BannedMembers = { "System.String::GetHashCode" };

    private static readonly string[] AllowedAssemblyReferences = { "netstandard" };

    [Fact]
    public void SimAssemblyIsPure()
    {
        List<string> violations = Scan(typeof(World).Assembly.Location);
        Assert.True(violations.Count == 0, "Sim.dll violates determinism rules:\n" + string.Join("\n", violations));
    }

    /// <summary>Сторож детектора: на сборке тестов (там double и Math есть) сканер обязан что-то найти.</summary>
    [Fact]
    public void ScannerDetectsViolationsInTestAssembly()
    {
        double probe = Math.Sqrt(2.0);
        Assert.True(probe > 1);
        List<string> violations = Scan(typeof(SimPurityTests).Assembly.Location);
        Assert.Contains(violations, v => v.Contains("System.Math", StringComparison.Ordinal));
        Assert.Contains(violations, v => v.Contains("float/double", StringComparison.Ordinal));
    }

    private static List<string> Scan(string path)
    {
        var violations = new List<string>();
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);
        MetadataReader md = pe.GetMetadataReader();
        var floats = new FloatDetector(md);

        foreach (AssemblyReferenceHandle handle in md.AssemblyReferences)
        {
            string name = md.GetString(md.GetAssemblyReference(handle).Name);
            if (!AllowedAssemblyReferences.Contains(name))
            {
                violations.Add($"assembly reference: {name}");
            }
        }
        foreach (TypeReferenceHandle handle in md.TypeReferences)
        {
            string name = FullName(md, handle);
            if (BannedTypes.Contains(name) || BannedNamespacePrefixes.Any(p => name.StartsWith(p + ".", StringComparison.Ordinal)))
            {
                violations.Add($"type reference: {name}");
            }
        }
        foreach (MemberReferenceHandle handle in md.MemberReferences)
        {
            MemberReference member = md.GetMemberReference(handle);
            string owner = member.Parent.Kind == HandleKind.TypeReference ? FullName(md, (TypeReferenceHandle)member.Parent) : "";
            string name = owner + "::" + md.GetString(member.Name);
            if (BannedMembers.Contains(name))
            {
                violations.Add($"member reference: {name}");
            }
            bool usesFloat = member.GetKind() == MemberReferenceKind.Method
                ? floats.Method(member.DecodeMethodSignature(floats, null))
                : member.DecodeFieldSignature(floats, null);
            if (usesFloat)
            {
                violations.Add($"float/double in member reference signature: {name}");
            }
        }
        for (int row = 1; row <= md.GetTableRowCount(TableIndex.TypeSpec); row++)
        {
            if (md.GetTypeSpecification(MetadataTokens.TypeSpecificationHandle(row)).DecodeSignature(floats, null))
            {
                violations.Add("float/double in a generic type instantiation");
            }
        }
        for (int row = 1; row <= md.GetTableRowCount(TableIndex.MethodSpec); row++)
        {
            if (md.GetMethodSpecification(MetadataTokens.MethodSpecificationHandle(row)).DecodeSignature(floats, null).Any(t => t))
            {
                violations.Add("float/double in a generic method instantiation");
            }
        }
        foreach (TypeDefinitionHandle typeHandle in md.TypeDefinitions)
        {
            TypeDefinition type = md.GetTypeDefinition(typeHandle);
            string typeName = md.GetString(type.Namespace) + "." + md.GetString(type.Name);
            foreach (FieldDefinitionHandle fieldHandle in type.GetFields())
            {
                FieldDefinition field = md.GetFieldDefinition(fieldHandle);
                if (field.DecodeSignature(floats, null))
                {
                    violations.Add($"float/double field: {typeName}.{md.GetString(field.Name)}");
                }
            }
            foreach (MethodDefinitionHandle methodHandle in type.GetMethods())
            {
                MethodDefinition method = md.GetMethodDefinition(methodHandle);
                string methodName = typeName + "." + md.GetString(method.Name);
                if (floats.Method(method.DecodeSignature(floats, null)))
                {
                    violations.Add($"float/double in method signature: {methodName}");
                }
                if (method.RelativeVirtualAddress == 0)
                {
                    continue;
                }
                MethodBodyBlock body = pe.GetMethodBody(method.RelativeVirtualAddress);
                if (!body.LocalSignature.IsNil)
                {
                    StandaloneSignature locals = md.GetStandaloneSignature(body.LocalSignature);
                    if (locals.DecodeLocalSignature(floats, null).Any(t => t))
                    {
                        violations.Add($"float/double local in: {methodName}");
                    }
                }
                foreach (string opcode in FloatOpcodes(body.GetILBytes()!))
                {
                    violations.Add($"float/double opcode {opcode} in: {methodName}");
                }
            }
        }
        return violations.Distinct().ToList();
    }

    private static string FullName(MetadataReader md, TypeReferenceHandle handle)
    {
        TypeReference type = md.GetTypeReference(handle);
        string name = md.GetString(type.Name);
        if (type.ResolutionScope.Kind == HandleKind.TypeReference)
        {
            return FullName(md, (TypeReferenceHandle)type.ResolutionScope) + "/" + name;
        }
        string ns = md.GetString(type.Namespace);
        return ns.Length == 0 ? name : ns + "." + name;
    }

    private static readonly Dictionary<short, OpCode> OpCodesByValue = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Select(f => (OpCode)f.GetValue(null)!)
        .ToDictionary(op => op.Value);

    private static readonly HashSet<string> FloatOpcodeNames = new()
    {
        "ldc.r4", "ldc.r8", "conv.r4", "conv.r8", "conv.r.un", "ldind.r4", "ldind.r8", "stind.r4", "stind.r8",
        "ldelem.r4", "ldelem.r8", "stelem.r4", "stelem.r8", "ckfinite",
    };

    private static IEnumerable<string> FloatOpcodes(byte[] il)
    {
        int i = 0;
        while (i < il.Length)
        {
            short value = il[i] == 0xFE ? (short)(0xFE00 | il[i + 1]) : il[i];
            i += il[i] == 0xFE ? 2 : 1;
            OpCode op = OpCodesByValue[value];
            if (FloatOpcodeNames.Contains(op.Name!))
            {
                yield return op.Name!;
            }
            i += op.OperandType switch
            {
                OperandType.InlineNone => 0,
                OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                OperandType.InlineVar => 2,
                OperandType.InlineI8 or OperandType.InlineR => 8,
                OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(il, i),
                _ => 4,
            };
        }
    }

    /// <summary>Провайдер сигнатур: true, если тип содержит float или double на любой глубине.</summary>
    private sealed class FloatDetector : ISignatureTypeProvider<bool, object?>
    {
        private readonly MetadataReader _md;

        public FloatDetector(MetadataReader md) => _md = md;

        public bool Method(MethodSignature<bool> signature) => signature.ReturnType || signature.ParameterTypes.Any(t => t);

        public bool GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode is PrimitiveTypeCode.Single or PrimitiveTypeCode.Double;
        public bool GetSZArrayType(bool elementType) => elementType;
        public bool GetArrayType(bool elementType, ArrayShape shape) => elementType;
        public bool GetByReferenceType(bool elementType) => elementType;
        public bool GetPointerType(bool elementType) => elementType;
        public bool GetPinnedType(bool elementType) => elementType;
        public bool GetGenericInstantiation(bool genericType, ImmutableArray<bool> typeArguments) => genericType || typeArguments.Any(t => t);
        public bool GetGenericMethodParameter(object? genericContext, int index) => false;
        public bool GetGenericTypeParameter(object? genericContext, int index) => false;
        public bool GetFunctionPointerType(MethodSignature<bool> signature) => Method(signature);
        public bool GetModifiedType(bool modifier, bool unmodifiedType, bool isRequired) => unmodifiedType;
        public bool GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) => false;
        public bool GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            string name = FullName(reader, handle);
            return name is "System.Single" or "System.Double";
        }

        public bool GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind) =>
            reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
    }
}
