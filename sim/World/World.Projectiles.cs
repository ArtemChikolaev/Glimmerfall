using Glimmerfall.Sim.Core;
using Glimmerfall.Sim.Determinism;
using Glimmerfall.Sim.Numerics;

namespace Glimmerfall.Sim;

public sealed partial class World
{
    private void UpdateProjectiles()
    {
        ProjectilePool p = Projectiles;
        EnemyPool e = Enemies;
        int[] start = Grid.CellStart;
        int[] items = Grid.Items;
        for (int i = 0; i < p.HighWater; i++)
        {
            if (!p.Alive[i])
            {
                continue;
            }
            Fix x = p.X[i] + p.VX[i];
            Fix y = p.Y[i] + p.VY[i];
            p.X[i] = x;
            p.Y[i] = y;
            if (--p.Life[i] <= 0 || x.Raw < 0 || y.Raw < 0 || x >= ArenaMax || y >= ArenaMax)
            {
                p.Release(i);
                continue;
            }
            int cx = SpatialGrid.CellCoord(x);
            int cy = SpatialGrid.CellCoord(y);
            int x0 = cx > 0 ? cx - 1 : 0;
            int x1 = cx + 1 < SpatialGrid.Cells ? cx + 1 : cx;
            int y0 = cy > 0 ? cy - 1 : 0;
            int y1 = cy + 1 < SpatialGrid.Cells ? cy + 1 : cy;
            long pr = p.Radius[i].Raw;
            int hit = -1;
            for (int gy = y0; gy <= y1 && hit < 0; gy++)
            {
                for (int gx = x0; gx <= x1 && hit < 0; gx++)
                {
                    int cell = gy * SpatialGrid.Cells + gx;
                    int end = start[cell + 1];
                    for (int k = start[cell]; k < end; k++)
                    {
                        int j = items[k];
                        if (!e.Alive[j])
                        {
                            continue;
                        }
                        long dx = e.X[j].Raw - x.Raw;
                        long dy = e.Y[j].Raw - y.Raw;
                        long rs = e.Radius[j].Raw + pr;
                        if (dx * dx + dy * dy >= rs * rs || e.Handle(j) == p.LastHit[i])
                        {
                            continue;
                        }
                        hit = j;
                        break;
                    }
                }
            }
            if (hit < 0)
            {
                continue;
            }
            int damage = p.Damage[i];
            bool crit = Rng[RngStream.Combat].Chance(Fix.Ratio(Rules.CritPercent, 100));
            if (crit)
            {
                damage *= 2;
            }
            p.LastHit[i] = e.Handle(hit);
            DamageEnemy(hit, damage, p.VX[i], p.VY[i], crit, p.HeroKind[i]);
            if (--p.Pierce[i] <= 0)
            {
                p.Release(i);
            }
        }
    }

    private void UpdateShots()
    {
        ShotPool s = Shots;
        long hx = Squad.X.Raw;
        long hy = Squad.Y.Raw;
        long heroRadius = ActiveHero.RadiusPx.Raw;
        for (int i = 0; i < s.HighWater; i++)
        {
            if (!s.Alive[i])
            {
                continue;
            }
            Fix x = s.X[i] + s.VX[i];
            Fix y = s.Y[i] + s.VY[i];
            s.X[i] = x;
            s.Y[i] = y;
            if (--s.Life[i] <= 0 || x.Raw < 0 || y.Raw < 0 || x >= ArenaMax || y >= ArenaMax)
            {
                s.Release(i);
                continue;
            }
            long dx = hx - x.Raw;
            long dy = hy - y.Raw;
            long rs = heroRadius + s.Radius[i].Raw;
            if (dx * dx + dy * dy >= rs * rs)
            {
                continue;
            }
            int damage = s.Damage[i];
            int source = s.SourceKind[i];
            s.Release(i);
            DamageSquad(damage, source);
            if (Outcome != RunOutcome.InProgress)
            {
                return;
            }
        }
    }
}
