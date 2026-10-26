namespace Glimmerfall.Sim.Content;

/// <summary>
/// Благословения спайка — общие на отряд. Финальная таксономия по ролям действий (PRD §6) — открытый вопрос;
/// здесь минимальный пул, чтобы модалка выбора и бот были настоящими.
/// </summary>
public enum Blessing : byte
{
    Might = 0,
    Haste = 1,
    Volley = 2,
    Pierce = 3,
    Swift = 4,
    Vigor = 5,
    Magnet = 6,
    Mend = 7,
}

public static class Blessings
{
    public const int Count = 8;

    private static readonly int[] MaxLevels = { 5, 5, 3, 3, 4, 5, 3, 99 };

    public static int MaxLevel(Blessing blessing) => MaxLevels[(int)blessing];
}

/// <summary>Варианты модалки чекпоинта между актами.</summary>
public enum CheckpointOption : byte
{
    Heal = 0,
    Revive = 1,
    Might = 2,
}
