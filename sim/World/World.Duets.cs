using Glimmerfall.Sim.Content;
using Glimmerfall.Sim.Core;
using Glimmerfall.Sim.Numerics;

namespace Glimmerfall.Sim;

/// <summary>
/// Дуэт-приёмы связки (PRD §5): свап на героя, который только что выпрыгнул ассистом, — пара бьёт смесью стихий.
/// Строятся из тех же примитивов, что ассист и вход героя: удар по кругу, отброс, удар по выбранным врагам.
/// Перебор врагов — по слотам, равные расстояния — меньший слот: порядок детерминирован.
/// </summary>
public sealed partial class World
{
    /// <summary>Центры взрывов огненного шторма (сырые координаты) — рабочий буфер, не состояние.</summary>
    private readonly long[] _blastX = new long[Rules.FirestormBlasts];
    private readonly long[] _blastY = new long[Rules.FirestormBlasts];

    private void Duet(DuetKind duet, HeroKind outgoing, HeroKind incoming)
    {
        Squad s = Squad;
        int damage = Rules.DuetDamage(outgoing, incoming, BlessingLevels[(int)Blessing.Might], Config.Meta);
        Stats.Acts[ActIndex].Duets++;
        Emit(SimEventType.Duet, s.X, s.Y, (int)duet, Rules.DuetRadius(duet).FloorToInt(), s.FacingX, s.FacingY,
            target: outgoing.Id, source: incoming.Id);
        switch (duet)
        {
            case DuetKind.SteamBurst:
                StrikeCircle(s.X, s.Y, Rules.SteamBurstRadiusPx, damage, incoming.Id);
                Knockback(Rules.SteamBurstRadiusPx, Rules.SteamBurstPushPx);
                break;
            case DuetKind.IceStorm:
                IceStorm(damage, incoming.Id);
                break;
            case DuetKind.Firestorm:
                Firestorm(damage, incoming.Id);
                break;
        }
    }

    /// <summary>
    /// Ледяная гроза: до <see cref="Rules.IceStormTargets"/> ударов с неба по разным врагам в досягаемости — сначала
    /// элиты и босс (ближние первыми), затем ближайшие рядовые.
    /// </summary>
    private void IceStorm(int damage, int source)
    {
        EnemyPool e = Enemies;
        long reach = Rules.IceStormRangePx.Raw;
        long r2 = reach * reach;
        _chainHits.Clear();
        for (int bolt = 0; bolt < Rules.IceStormTargets; bolt++)
        {
            int target = -1;
            bool targetHeavy = false;
            long best = r2 + 1;
            for (int i = 0; i < e.HighWater; i++)
            {
                if (!e.Alive[i] || _chainHits.Contains(i))
                {
                    continue;
                }
                long dx = e.X[i].Raw - Squad.X.Raw;
                long dy = e.Y[i].Raw - Squad.Y.Raw;
                long d2 = dx * dx + dy * dy;
                bool heavy = _heavyKind[e.Kind[i]];
                if (d2 > r2 || (targetHeavy && !heavy))
                {
                    continue;
                }
                if (d2 < best || (heavy && !targetHeavy))
                {
                    best = d2;
                    target = i;
                    targetHeavy = heavy;
                }
            }
            if (target < 0)
            {
                break;
            }
            _chainHits.Add(target);
            Emit(SimEventType.DuetStrike, e.X[target], e.Y[target], (int)DuetKind.IceStorm, 0, source: source);
            DamageEnemy(target, damage, e.X[target] - Squad.X, e.Y[target] - Squad.Y, false, source);
        }
    }

    /// <summary>
    /// Огненный шторм: до <see cref="Rules.FirestormBlasts"/> взрывов в точках врагов в досягаемости — от ближних
    /// к дальним, центры не ближе <see cref="Rules.FirestormSpacingPx"/> друг к другу. Центры выбираются до урона.
    /// </summary>
    private void Firestorm(int damage, int source)
    {
        EnemyPool e = Enemies;
        long reach = Rules.FirestormRangePx.Raw;
        long r2 = reach * reach;
        long spacing = Rules.FirestormSpacingPx.Raw;
        int blasts = 0;
        while (blasts < Rules.FirestormBlasts)
        {
            int pick = -1;
            long best = r2 + 1;
            for (int i = 0; i < e.HighWater; i++)
            {
                if (!e.Alive[i])
                {
                    continue;
                }
                long dx = e.X[i].Raw - Squad.X.Raw;
                long dy = e.Y[i].Raw - Squad.Y.Raw;
                long d2 = dx * dx + dy * dy;
                if (d2 >= best || NearBlast(e.X[i].Raw, e.Y[i].Raw, blasts, spacing * spacing))
                {
                    continue;
                }
                best = d2;
                pick = i;
            }
            if (pick < 0)
            {
                break;
            }
            _blastX[blasts] = e.X[pick].Raw;
            _blastY[blasts] = e.Y[pick].Raw;
            blasts++;
        }
        for (int b = 0; b < blasts; b++)
        {
            Fix x = Fix.FromRaw(_blastX[b]);
            Fix y = Fix.FromRaw(_blastY[b]);
            Emit(SimEventType.DuetStrike, x, y, (int)DuetKind.Firestorm, Rules.FirestormBlastRadiusPx.FloorToInt(), source: source);
            StrikeCircle(x, y, Rules.FirestormBlastRadiusPx, damage, source);
        }
    }

    /// <summary>Точка ближе допустимого к одному из уже выбранных центров взрыва.</summary>
    private bool NearBlast(long x, long y, int count, long spacingSquared)
    {
        for (int b = 0; b < count; b++)
        {
            long dx = x - _blastX[b];
            long dy = y - _blastY[b];
            if (dx * dx + dy * dy < spacingSquared)
            {
                return true;
            }
        }
        return false;
    }
}
