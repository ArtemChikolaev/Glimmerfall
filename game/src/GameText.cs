using System.Globalization;
using Glimmerfall.Sim;
using Glimmerfall.Sim.Content;
using Glimmerfall.Sim.Numerics;

namespace Glimmerfall.Game;

/// <summary>
/// Тексты прототипа: имена, благословения и цели. Числа в описаниях считаются функциями сима (<see cref="Rules"/>) —
/// подсказка не может разойтись с правилом. Имена и лор — рабочие, финальные решает владелец (PRD: «Открыто»).
/// </summary>
public static class GameText
{
    public static readonly string[] HeroNames = { "Фонарщик", "Страж", "Искровед" };
    public static readonly string[] HeroElements = { "огонь", "мороз", "молния" };
    public static readonly string[] EnemyNames = { "Бродяга", "Бегун", "Громила", "Плевальщик", "Таран", "Опустошитель", "Страж мрака" };
    public static readonly string[] ActNames = { "Угасающий тракт", "Погост", "Собор Тьмы", "Пепельный предел" };

    public static string ActName(int act) => ActNames[act % ActNames.Length];

    public static string Clock(int ticks)
    {
        int seconds = System.Math.Max(0, ticks) / SimConstants.TicksPerSecond;
        return string.Format(CultureInfo.InvariantCulture, "{0}:{1:00}", seconds / 60, seconds % 60);
    }

    public static string BlessingTitle(Blessing blessing) => blessing switch
    {
        Blessing.Might => "Мощь",
        Blessing.Haste => "Спешка",
        Blessing.Volley => "Залп",
        Blessing.Pierce => "Пронзание",
        Blessing.Swift => "Лёгкий шаг",
        Blessing.Vigor => "Стойкость",
        Blessing.Magnet => "Притяжение",
        Blessing.Mend => "Перевязка",
        _ => blessing.ToString(),
    };

    /// <summary>Что даст следующий уровень — для активного героя, числами из правил сима.</summary>
    public static string BlessingDescription(Blessing blessing, World world)
    {
        HeroKind hero = world.ActiveHero;
        int level = world.BlessingLevels[(int)blessing];
        MetaLoadout meta = world.Config.Meta;
        switch (blessing)
        {
            case Blessing.Might:
                return $"Урон всех героев +15%.\n{HeroNames[hero.Id]}: {Rules.HeroDamage(hero, level, meta)} → {Rules.HeroDamage(hero, level + 1, meta)}";
            case Blessing.Haste:
                return $"Атаки на 8% чаще.\nПауза между атаками: {Seconds(Rules.AttackInterval(hero, level))} → {Seconds(Rules.AttackInterval(hero, level + 1))} с";
            case Blessing.Volley:
                return $"Больше снарядов в атаке.\n{HeroNames[hero.Id]}: {Rules.ProjectileCount(hero, level)} → {Rules.ProjectileCount(hero, level + 1)}";
            case Blessing.Pierce:
                return $"Снаряд пробивает ещё одного врага.\nПробитие: {Rules.Pierce(hero, level)} → {Rules.Pierce(hero, level + 1)}";
            case Blessing.Swift:
                return $"Скорость отряда +8%.\n{PerSecond(Rules.MoveSpeed(hero, level, meta))} → {PerSecond(Rules.MoveSpeed(hero, level + 1, meta))} px/с";
            case Blessing.Vigor:
                return $"Здоровье каждого героя +{Rules.VigorHpPerLevel}.\n{HeroNames[hero.Id]}: {Rules.HeroMaxHp(hero, level, meta)} → {Rules.HeroMaxHp(hero, level + 1, meta)}";
            case Blessing.Magnet:
                return $"Опыт притягивается издалека.\nРадиус: {Rules.PickupRadius(level, meta).FloorToInt()} → {Rules.PickupRadius(level + 1, meta).FloorToInt()} px";
            case Blessing.Mend:
                return $"Лечит активного героя на {Rules.MendHealPercent}% здоровья.";
            default:
                return "";
        }
    }

    public static string CheckpointTitle(CheckpointOption option) => option switch
    {
        CheckpointOption.Heal => "Привал",
        CheckpointOption.Revive => "Поднять павших",
        CheckpointOption.Might => "Закалка",
        _ => option.ToString(),
    };

    public static string CheckpointDescription(CheckpointOption option) => option switch
    {
        CheckpointOption.Heal => $"Лечит весь отряд на {Rules.CheckpointHealPercent}% здоровья.",
        CheckpointOption.Revive => $"Павшие герои встают с {Rules.ReviveHpPercent}% здоровья.",
        CheckpointOption.Might => "Урон всех героев +15% (уровень «Мощи»).",
        _ => "",
    };

    /// <summary>Цель на экране: до босса — дожить, при живом боссе — победить, после конца акта — босс в ярости.</summary>
    public static string Objective(World world)
    {
        int bossTick = world.Config.ActTicks - world.Config.BossLeadTicks;
        if (world.IsBossAlive)
        {
            return world.ActTick > world.Config.ActTicks ? "Страж мрака в ярости — добей его!" : "Победи Стража мрака";
        }
        return $"Продержись до прихода Стража мрака — {Clock(bossTick - world.ActTick)}";
    }

    private static string Seconds(int ticks) => (ticks / (float)SimConstants.TicksPerSecond).ToString("0.00", CultureInfo.InvariantCulture);

    private static string PerSecond(Fix perTick) => ((int)(perTick.Raw * SimConstants.TicksPerSecond / Fix.OneRaw)).ToString(CultureInfo.InvariantCulture);
}
