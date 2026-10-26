using System;
using System.Collections.Generic;
using System.Globalization;

namespace Glimmerfall.Sim.Cli;

/// <summary>
/// Разбор «--ключ значение». Флаг без значения не съедает следующий «--ключ» (урок sim_arcade: `--verbose --enemy x=1`
/// молча терял `--enemy`). Неизвестный ключ — ошибка: тихо проигнорированный оверрайд рисует «эффекта нет».
/// </summary>
public sealed class Args
{
    /// <summary>Булевы флаги никогда не берут значение: иначе `verify --json FILE` съедал бы путь к файлу.</summary>
    private static readonly HashSet<string> BooleanFlags = new() { "json", "verbose" };

    private readonly Dictionary<string, List<string>> _values = new();
    private readonly HashSet<string> _used = new();

    public readonly List<string> Positional = new();

    public Args(IEnumerable<string> args)
    {
        var list = new List<string>(args);
        for (int i = 0; i < list.Count; i++)
        {
            string token = list[i];
            if (!token.StartsWith("--", StringComparison.Ordinal))
            {
                Positional.Add(token);
                continue;
            }
            string key = token.Substring(2);
            string value = "";
            int eq = key.IndexOf('=');
            if (eq > 0 && !key.StartsWith("set", StringComparison.Ordinal))
            {
                value = key.Substring(eq + 1);
                key = key.Substring(0, eq);
            }
            else if (!BooleanFlags.Contains(key) && i + 1 < list.Count && !list[i + 1].StartsWith("--", StringComparison.Ordinal))
            {
                value = list[++i];
            }
            if (!_values.TryGetValue(key, out List<string>? values))
            {
                values = new List<string>();
                _values[key] = values;
            }
            values.Add(value);
        }
    }

    public bool Has(string key)
    {
        _used.Add(key);
        return _values.ContainsKey(key);
    }

    public string Get(string key, string fallback)
    {
        _used.Add(key);
        return _values.TryGetValue(key, out List<string>? values) ? values[values.Count - 1] : fallback;
    }

    public int GetInt(string key, int fallback)
    {
        string value = Get(key, "");
        return value.Length == 0 ? fallback : int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
    }

    public IReadOnlyList<string> GetAll(string key)
    {
        _used.Add(key);
        return _values.TryGetValue(key, out List<string>? values) ? values : (IReadOnlyList<string>)Array.Empty<string>();
    }

    /// <summary>Бросает, если остались ключи, которые команда не прочитала.</summary>
    public void EnsureAllUsed()
    {
        foreach (string key in _values.Keys)
        {
            if (!_used.Contains(key))
            {
                throw new ArgumentException($"unknown option --{key}");
            }
        }
    }
}
