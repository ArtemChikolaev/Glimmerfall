using System;
using System.Collections.Generic;
using System.Globalization;
using Glimmerfall.Sim.Content;
using Glimmerfall.Sim.Determinism;
using Glimmerfall.Sim.Numerics;

namespace Glimmerfall.Sim;

public enum Scenario : byte
{
    /// <summary>Забег: акты с директором спавна, элитами, боссом, опытом и модалками.</summary>
    Run = 0,
    /// <summary>Спайк орды (G1.3): держит заданное число врагов и снарядов, отряд неуязвим, без модалок.</summary>
    HordeBench = 1,
}

/// <summary>Мета-прокачка между забегами (карьерный режим). Входит в реплей: реплей не читает профиль зрителя.</summary>
public struct MetaLoadout : IEquatable<MetaLoadout>
{
    public const int MaxRank = 5;

    /// <summary>+5% урона за ранг.</summary>
    public byte Damage;
    /// <summary>+8% здоровья героев за ранг.</summary>
    public byte Health;
    /// <summary>+3% скорости за ранг.</summary>
    public byte Speed;
    /// <summary>+10% радиуса подбора за ранг.</summary>
    public byte Magnet;

    public readonly int TotalRanks => Damage + Health + Speed + Magnet;

    public readonly bool Equals(MetaLoadout other) =>
        Damage == other.Damage && Health == other.Health && Speed == other.Speed && Magnet == other.Magnet;

    public override readonly bool Equals(object? obj) => obj is MetaLoadout other && Equals(other);
    public override readonly int GetHashCode() => Damage | (Health << 8) | (Speed << 16) | (Magnet << 24);
    public override readonly string ToString() => $"dmg{Damage}.hp{Health}.spd{Speed}.mag{Magnet}";
}

/// <summary>
/// Всё, что влияет на исход забега, кроме сида и ввода. Реплей хранит это целиком (сценарий, акты, тир, отряд, мету и оверрайды).
/// Оверрайды <c>--set</c> — только для A/B-замеров: в игру попадает правка данных.
/// </summary>
public sealed class SimConfig
{
    public Scenario Scenario = Scenario.Run;
    public int Acts = 3;
    public int ActTicks = 8 * 60 * SimConstants.TicksPerSecond;
    /// <summary>Босс появляется за столько тиков до конца акта; акт закрывается его смертью.</summary>
    public int BossLeadTicks = 60 * SimConstants.TicksPerSecond;
    /// <summary>Через столько тиков после конца акта живой босс впадает в ярость (×2 скорость и урон).</summary>
    public int BossEnrageTicks = 120 * SimConstants.TicksPerSecond;
    /// <summary>Тир лестницы сложности, 0 — базовый.</summary>
    public int Tier;
    public MetaLoadout Meta;
    public int[] Squad = { HeroIds.Lamplighter, HeroIds.Warden, HeroIds.Sparkwright };

    public int MaxEnemies = 800;
    /// <summary>Темп спавна, тысячные доли врага в секунду, в начале и в конце акта (линейно).</summary>
    public int SpawnRateStartMilli = 1600;
    public int SpawnRateEndMilli = 7000;
    /// <summary>Прибавка темпа за каждый следующий акт, проценты.</summary>
    public int SpawnPerActPercent = 35;
    /// <summary>Первая элита и период элит, тики времени акта.</summary>
    public int EliteFirstTicks = 120 * SimConstants.TicksPerSecond;
    public int ElitePeriodTicks = 120 * SimConstants.TicksPerSecond;

    public int BenchEnemies = 5000;
    public int BenchProjectiles = 2000;

    public EnemyKind[] EnemyKinds = SpikeContent.CreateEnemyKinds();
    public HeroKind[] HeroKinds = SpikeContent.CreateHeroKinds();

    private readonly List<string> _overrides = new();

    /// <summary>Применённые оверрайды «ключ=значение» в порядке применения (для отчётов и реплея).</summary>
    public IReadOnlyList<string> Overrides => _overrides;

    public static SimConfig Run(int acts = 3) => new() { Scenario = Scenario.Run, Acts = acts };

    public static SimConfig HordeBench(int enemies = 5000, int projectiles = 2000) => new()
    {
        Scenario = Scenario.HordeBench,
        Acts = 1,
        ActTicks = int.MaxValue / 2,
        BenchEnemies = enemies,
        BenchProjectiles = projectiles,
        MaxEnemies = enemies,
    };

    /// <summary>
    /// Применяет оверрайд вида <c>ключ=значение</c>. Неизвестный ключ — исключение: «одинаковые числа во всех клетках сетки»
    /// чаще значат, что оверрайд не применился, чем «эффекта нет» (скилл balance-harness).
    /// </summary>
    public void ApplyOverride(string assignment)
    {
        int eq = assignment.IndexOf('=');
        if (eq <= 0)
        {
            throw new ArgumentException($"override must be key=value: '{assignment}'");
        }
        string key = assignment.Substring(0, eq).Trim();
        if (!int.TryParse(assignment.Substring(eq + 1).Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int value))
        {
            throw new ArgumentException($"override value must be an integer: '{assignment}'");
        }
        if (!TryApply(key, value))
        {
            throw new ArgumentException($"unknown override key '{key}'. Known: {string.Join(", ", KnownKeys)}");
        }
        _overrides.Add(key + "=" + value.ToString(CultureInfo.InvariantCulture));
    }

    private bool TryApply(string key, int value)
    {
        switch (key)
        {
            case "act.seconds": ActTicks = value * SimConstants.TicksPerSecond; return true;
            case "act.count": Acts = value; return true;
            case "spawn.max": MaxEnemies = value; return true;
            case "spawn.startMilli": SpawnRateStartMilli = value; return true;
            case "spawn.endMilli": SpawnRateEndMilli = value; return true;
            case "spawn.perActPercent": SpawnPerActPercent = value; return true;
            case "elite.firstSeconds": EliteFirstTicks = value * SimConstants.TicksPerSecond; return true;
            case "elite.periodSeconds": ElitePeriodTicks = value * SimConstants.TicksPerSecond; return true;
            case "boss.leadSeconds": BossLeadTicks = value * SimConstants.TicksPerSecond; return true;
        }
        string[] parts = key.Split('.');
        if (parts.Length == 3 && parts[0] == "enemy")
        {
            EnemyKind? kind = Array.Find(EnemyKinds, k => k.Name == parts[1]);
            if (kind == null)
            {
                return false;
            }
            switch (parts[2])
            {
                case "hp": kind.Hp = value; return true;
                case "damage": kind.ContactDamage = value; return true;
                case "speed": kind.SpeedPxPerTick = Fix.PerSecond(value); return true;
                case "xp": kind.Xp = value; return true;
            }
            return false;
        }
        if (parts.Length == 3 && parts[0] == "hero")
        {
            HeroKind? kind = Array.Find(HeroKinds, k => k.Name == parts[1]);
            if (kind == null)
            {
                return false;
            }
            switch (parts[2])
            {
                case "hp": kind.MaxHp = value; return true;
                case "damage": kind.Damage = value; return true;
                case "speed": kind.SpeedPxPerTick = Fix.PerSecond(value); return true;
                case "interval": kind.AttackIntervalTicks = value; return true;
            }
            return false;
        }
        return false;
    }

    /// <summary>
    /// Поля верхнего уровня одной строкой «ключ=значение» (заголовок реплея). Виды — через оверрайды и отпечаток.
    /// Числа — в инвариантной культуре: иначе на шведской или финской локали минус стал бы U+2212 и сломал реплей.
    /// </summary>
    public string ToHeader()
    {
        var parts = new List<string>
        {
            "scenario=" + I((int)Scenario),
            "acts=" + I(Acts),
            "actTicks=" + I(ActTicks),
            "bossLeadTicks=" + I(BossLeadTicks),
            "bossEnrageTicks=" + I(BossEnrageTicks),
            "tier=" + I(Tier),
            "meta=" + I(Meta.Damage) + "," + I(Meta.Health) + "," + I(Meta.Speed) + "," + I(Meta.Magnet),
            "squad=" + I(Squad[0]) + "," + I(Squad[1]) + "," + I(Squad[2]),
            "maxEnemies=" + I(MaxEnemies),
            "spawnStartMilli=" + I(SpawnRateStartMilli),
            "spawnEndMilli=" + I(SpawnRateEndMilli),
            "spawnPerActPercent=" + I(SpawnPerActPercent),
            "eliteFirstTicks=" + I(EliteFirstTicks),
            "elitePeriodTicks=" + I(ElitePeriodTicks),
            "benchEnemies=" + I(BenchEnemies),
            "benchProjectiles=" + I(BenchProjectiles),
        };
        return string.Join(" ", parts);
    }

    private static string I(int value) => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>Конфиг из реплея — чужой ввод: значения, на которых сим упал бы (ноль актов, деление на ноль, чужой герой), — отказ.</summary>
    private void Validate()
    {
        bool ok = Acts >= 1 && Acts <= 16
            && ActTicks >= SimConstants.TicksPerSecond
            && BossLeadTicks >= 0 && BossEnrageTicks >= 0
            && Tier >= 0
            && Meta.Damage <= MetaLoadout.MaxRank && Meta.Health <= MetaLoadout.MaxRank
            && Meta.Speed <= MetaLoadout.MaxRank && Meta.Magnet <= MetaLoadout.MaxRank
            && MaxEnemies >= 1 && MaxEnemies <= World.EnemyCapacity
            && SpawnRateStartMilli >= 0 && SpawnRateEndMilli >= 0
            && EliteFirstTicks >= 1 && ElitePeriodTicks >= 1
            && BenchEnemies >= 0 && BenchEnemies <= World.EnemyCapacity
            && BenchProjectiles >= 0 && BenchProjectiles <= World.ProjectileCapacity
            && (Scenario == Scenario.Run || Scenario == Scenario.HordeBench);
        foreach (int hero in Squad)
        {
            ok &= hero >= 0 && hero < HeroKinds.Length;
        }
        if (!ok)
        {
            throw new FormatException("replay config is out of range: " + ToHeader());
        }
    }

    /// <summary>Восстанавливает конфиг из заголовка и оверрайдов (обратное к <see cref="ToHeader"/>).</summary>
    public static SimConfig FromHeader(string header, IEnumerable<string> overrides)
    {
        var config = new SimConfig();
        foreach (string token in header.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
        {
            int eq = token.IndexOf('=');
            if (eq <= 0)
            {
                throw new FormatException($"bad config token '{token}'");
            }
            string key = token.Substring(0, eq);
            string value = token.Substring(eq + 1);
            switch (key)
            {
                case "scenario": config.Scenario = (Scenario)ParseInt(value); break;
                case "acts": config.Acts = ParseInt(value); break;
                case "actTicks": config.ActTicks = ParseInt(value); break;
                case "bossLeadTicks": config.BossLeadTicks = ParseInt(value); break;
                case "bossEnrageTicks": config.BossEnrageTicks = ParseInt(value); break;
                case "tier": config.Tier = ParseInt(value); break;
                case "meta":
                    int[] meta = ParseList(value, 4);
                    config.Meta = new MetaLoadout { Damage = (byte)meta[0], Health = (byte)meta[1], Speed = (byte)meta[2], Magnet = (byte)meta[3] };
                    break;
                case "squad": config.Squad = ParseList(value, 3); break;
                case "maxEnemies": config.MaxEnemies = ParseInt(value); break;
                case "spawnStartMilli": config.SpawnRateStartMilli = ParseInt(value); break;
                case "spawnEndMilli": config.SpawnRateEndMilli = ParseInt(value); break;
                case "spawnPerActPercent": config.SpawnPerActPercent = ParseInt(value); break;
                case "eliteFirstTicks": config.EliteFirstTicks = ParseInt(value); break;
                case "elitePeriodTicks": config.ElitePeriodTicks = ParseInt(value); break;
                case "benchEnemies": config.BenchEnemies = ParseInt(value); break;
                case "benchProjectiles": config.BenchProjectiles = ParseInt(value); break;
                default: throw new FormatException($"unknown config key '{key}'");
            }
        }
        foreach (string assignment in overrides)
        {
            try
            {
                config.ApplyOverride(assignment);
            }
            catch (ArgumentException e)
            {
                throw new FormatException(e.Message);
            }
        }
        config.Validate();
        return config;
    }

    /// <summary>
    /// Отпечаток всего, что влияет на исход: поля верхнего уровня и все числа видов. Реплей хранит его и отказывается
    /// воспроизводиться, если данные изменились без бампа версии баланса.
    /// </summary>
    public ulong ComputeFingerprint()
    {
        var h = new StateHasher();
        foreach (char c in ToHeader())
        {
            h.Add((int)c);
        }
        foreach (EnemyKind k in EnemyKinds)
        {
            h.Add(k.Id);
            h.Add((int)k.Behavior);
            h.Add(k.Elite);
            h.Add(k.RadiusPx.Raw);
            h.Add(k.SpeedPxPerTick.Raw);
            h.Add(k.Hp);
            h.Add(k.ContactDamage);
            h.Add(k.Xp);
            h.Add(k.PreferredRangePx.Raw);
            h.Add(k.AimTicks);
            h.Add(k.FireCooldownTicks);
            h.Add(k.ShotSpeedPxPerTick.Raw);
            h.Add(k.ShotDamage);
            h.Add(k.ChargeTriggerPx.Raw);
            h.Add(k.ChargeWindupTicks);
            h.Add(k.ChargeTicks);
            h.Add(k.ChargeSpeedPxPerTick.Raw);
            h.Add(k.ChargeCooldownTicks);
            h.Add(k.SlamRadiusPx.Raw);
            h.Add(k.SlamWindupTicks);
            h.Add(k.SlamCooldownTicks);
            h.Add(k.SlamDamage);
        }
        foreach (HeroKind k in HeroKinds)
        {
            h.Add(k.Id);
            h.Add(k.MaxHp);
            h.Add(k.SpeedPxPerTick.Raw);
            h.Add(k.RadiusPx.Raw);
            h.Add((int)k.Element);
            h.Add((int)k.Assist);
            h.Add((int)k.Pattern);
            h.Add(k.AttackIntervalTicks);
            h.Add(k.ProjectileCount);
            h.Add(k.SpreadAngle);
            h.Add(k.Damage);
            h.Add(k.ProjectileSpeedPxPerTick.Raw);
            h.Add(k.RangePx.Raw);
            h.Add(k.Pierce);
            h.Add(k.ProjectileRadiusPx.Raw);
        }
        return h.Finish();
    }

    private static int ParseInt(string value) =>
        int.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int result)
            ? result
            : throw new FormatException($"bad number '{value}'");

    private static int[] ParseList(string value, int expected)
    {
        string[] parts = value.Split(',');
        if (parts.Length != expected)
        {
            throw new FormatException($"expected {expected} values in '{value}'");
        }
        var result = new int[expected];
        for (int i = 0; i < expected; i++)
        {
            result[i] = ParseInt(parts[i]);
        }
        return result;
    }

    /// <summary>Массив, а не итератор: <c>yield</c> тянет в сим Environment.CurrentManagedThreadId (ловит SimPurityTests).</summary>
    private static readonly string[] KnownKeys =
    {
        "act.seconds", "act.count", "spawn.max", "spawn.startMilli", "spawn.endMilli", "spawn.perActPercent",
        "elite.firstSeconds", "elite.periodSeconds", "boss.leadSeconds",
        "enemy.<name>.{hp,damage,speed,xp}", "hero.<name>.{hp,damage,speed,interval}",
    };
}
