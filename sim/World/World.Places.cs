using Glimmerfall.Sim.Content;
using Glimmerfall.Sim.Core;
using Glimmerfall.Sim.Determinism;
using Glimmerfall.Sim.Numerics;

namespace Glimmerfall.Sim;

/// <summary>
/// Места акта (PRD §4; скилл encounter-design: «места» спят до контакта). Святилище: постой в круге
/// <see cref="Rules.ShrineChannelTicks"/> — сила отряда на <see cref="Rules.ShrineBuffTicks"/>; вышел — прогресс тает.
/// Алтарь риска: касание будит стража (элиту) за алтарём и кольцо свиты; страж убит — дар: лишний выбор
/// благословения через общий конвейер предложений.
/// </summary>
public sealed partial class World
{
    /// <summary>Места нового акта: от сида (поток Places), вне круга старта и не теснее шага друг к другу.</summary>
    private void LayoutPlaces()
    {
        Places.Clear();
        ref Pcg32 rng = ref Rng[RngStream.Places];
        const int border = 320;
        int span = SimConstants.ArenaSize - 2 * border;
        for (int k = 0; k < Rules.ShrinesPerAct + Rules.AltarsPerAct; k++)
        {
            PlaceKind kind = k < Rules.ShrinesPerAct ? PlaceKind.Shrine : PlaceKind.Altar;
            for (int attempt = 0; attempt < 64; attempt++)
            {
                Fix x = Fix.FromInt(border + rng.NextInt(span));
                Fix y = Fix.FromInt(border + rng.NextInt(span));
                if (PlaceFits(x, y))
                {
                    Places.Add(kind, x, y);
                    break;
                }
            }
        }
    }

    private bool PlaceFits(Fix x, Fix y)
    {
        long center = Fix.FromInt(SimConstants.ArenaSize / 2).Raw;
        long keepOut = Rules.PlaceKeepOutPx.Raw;
        long dx = x.Raw - center;
        long dy = y.Raw - center;
        if (dx * dx + dy * dy < keepOut * keepOut)
        {
            return false;
        }
        long spacing = Rules.PlaceSpacingPx.Raw;
        for (int i = 0; i < Places.Count; i++)
        {
            long px = x.Raw - Places.X[i].Raw;
            long py = y.Raw - Places.Y[i].Raw;
            if (px * px + py * py < spacing * spacing)
            {
                return false;
            }
        }
        return true;
    }

    private void UpdatePlaces()
    {
        Squad s = Squad;
        if (s.ShrineTicks > 0)
        {
            s.ShrineTicks--;
        }
        long r = Rules.PlaceRadiusPx.Raw;
        for (int i = 0; i < Places.Count; i++)
        {
            long dx = Places.X[i].Raw - s.X.Raw;
            long dy = Places.Y[i].Raw - s.Y.Raw;
            bool inside = dx * dx + dy * dy <= r * r;
            if (Places.Kind[i] == PlaceKind.Shrine)
            {
                UpdateShrine(i, inside);
            }
            else
            {
                UpdateAltar(i, inside);
            }
        }
    }

    private void UpdateShrine(int i, bool inside)
    {
        if (Places.State[i] != PlaceState.Dormant)
        {
            return;
        }
        if (!inside)
        {
            Places.Progress[i] = Places.Progress[i] > 2 ? Places.Progress[i] - 2 : 0;
            return;
        }
        if (++Places.Progress[i] < Rules.ShrineChannelTicks)
        {
            return;
        }
        Places.State[i] = PlaceState.Spent;
        Squad.ShrineTicks = Rules.ShrineBuffTicks;
        Stats.Acts[ActIndex].Shrines++;
        Emit(SimEventType.PlaceUsed, Places.X[i], Places.Y[i], (int)PlaceKind.Shrine, Rules.ShrineBuffTicks);
    }

    private void UpdateAltar(int i, bool inside)
    {
        if (Places.State[i] == PlaceState.Dormant && inside)
        {
            WakeAltar(i);
        }
        else if (Places.State[i] == PlaceState.Awake && Places.Guardian[i] >= 0 && !Enemies.IsLive(Places.Guardian[i]))
        {
            Places.State[i] = PlaceState.Spent;
            Places.Guardian[i] = -1;
            PendingLevelUps++;
            Stats.Acts[ActIndex].Altars++;
            Emit(SimEventType.PlaceReward, Places.X[i], Places.Y[i], (int)PlaceKind.Altar);
        }
    }

    /// <summary>Страж встаёт за алтарём, дальше от героя, — миг увидеть его и отойти; свита — кольцом вокруг.</summary>
    private void WakeAltar(int i)
    {
        Places.State[i] = PlaceState.Awake;
        Fix x = Places.X[i];
        Fix y = Places.Y[i];
        int away = FixMath.Atan2(y.Raw - Squad.Y.Raw, x.Raw - Squad.X.Raw);
        QueueSpawnAt(EnemyIds.Ravager, x + FixMath.Cos(away) * Rules.AltarGuardianDistancePx,
            y + FixMath.Sin(away) * Rules.AltarGuardianDistancePx, boss: false, place: i);
        for (int k = 0; k < Rules.AltarEscort; k++)
        {
            int angle = away + k * FixMath.FullTurn / Rules.AltarEscort;
            QueueSpawnAt(EnemyIds.Shambler, x + FixMath.Cos(angle) * Rules.AltarEscortRadiusPx,
                y + FixMath.Sin(angle) * Rules.AltarEscortRadiusPx, boss: false);
        }
        Emit(SimEventType.PlaceAwake, x, y, (int)PlaceKind.Altar, 0, FixMath.Cos(away), FixMath.Sin(away));
    }

    /// <summary>Страж алтаря появился; нет места в пуле врагов — алтарь снова спит и проснётся при следующем касании.</summary>
    private void GuardianSpawned(int place, int slot)
    {
        if (slot >= 0)
        {
            Places.Guardian[place] = Enemies.Handle(slot);
        }
        else
        {
            Places.State[place] = PlaceState.Dormant;
        }
    }
}
