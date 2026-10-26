namespace Glimmerfall.Sim.Core;

/// <summary>Телеметрия одного акта для бот-харнесса. На логику не влияет и в хеш не входит.</summary>
public sealed class ActStats
{
    public bool Reached;
    public bool Cleared;
    public int StartTick;
    public int EndTick = -1;
    /// <summary>Время акта (тики), когда отряд пал в этом акте; −1 — не пал.</summary>
    public int WipeActTick = -1;
    public int Kills;
    public int EliteKills;
    public int DamageTaken;
    public long DamageDealt;
    public int HeroDeaths;
    public int Swaps;
    public int Dashes;
    public int Assists;
    public int Duets;
    public int LevelAtEnd;
    public int PeakEnemies;
}

public sealed class RunStats
{
    public readonly ActStats[] Acts;

    /// <summary>Вид врага, нанёсшего последний урон отряду (атрибуция смерти); −1 — нет.</summary>
    public int LastDamageKind = -1;
    public int KillerKind = -1;

    public RunStats(int acts)
    {
        Acts = new ActStats[acts];
        for (int i = 0; i < acts; i++)
        {
            Acts[i] = new ActStats();
        }
    }

    public int TotalKills
    {
        get
        {
            int n = 0;
            foreach (ActStats act in Acts)
            {
                n += act.Kills;
            }
            return n;
        }
    }
}
