namespace Glimmerfall.Sim;

/// <summary>Неизменяемые константы сима. Время — только тиками (скилл sim-determinism, правило 3).</summary>
public static class SimConstants
{
    public const int TicksPerSecond = 60;

    /// <summary>Сторона квадратной арены спайка в пикселях базовой сетки.</summary>
    public const int ArenaSize = 4096;

    /// <summary>Ячейка равномерной сетки коллизий, пиксели. Не меньше максимальной суммы радиусов снаряда и врага.</summary>
    public const int GridCellSize = 32;

    public const int GridCells = ArenaSize / GridCellSize;
}

/// <summary>Версия правил и коэффициентов. Изменил правило или число — бампни: несовместимый реплей отказывается явно.</summary>
public static class SimVersion
{
    /// <summary>0.2.0 — ассист резервного героя и удар входящего при смене; 0.3.0 — связка и дуэт-приёмы пар (PRD §5).</summary>
    public const string Balance = "0.3.0-link";
}
