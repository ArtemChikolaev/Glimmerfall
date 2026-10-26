using Glimmerfall.Sim.Numerics;

namespace Glimmerfall.Sim.Core;

/// <summary>
/// Равномерная сетка врагов, перестраиваемая каждый тик сортировкой подсчётом. Внутри ячейки враги идут
/// по возрастанию слота — порядок обхода стабилен на всех платформах (скилл sim-determinism, правило 4).
/// </summary>
public sealed class SpatialGrid
{
    public const int CellShift = 5; // GridCellSize = 32 = 2^5
    public const int Cells = SimConstants.GridCells;

    /// <summary>Враги ячейки c — Items[CellStart[c] .. CellStart[c + 1]).</summary>
    public readonly int[] CellStart = new int[Cells * Cells + 1];
    public readonly int[] Items;

    private readonly int[] _cellOf;
    private readonly int[] _cursor = new int[Cells * Cells];

    public SpatialGrid(int capacity)
    {
        Items = new int[capacity];
        _cellOf = new int[capacity];
    }

    public static int CellCoord(Fix value)
    {
        int c = (int)(value.Raw >> (Fix.FractionBits + CellShift));
        return c < 0 ? 0 : c >= Cells ? Cells - 1 : c;
    }

    public void Build(EnemyPool enemies)
    {
        int cellCount = Cells * Cells;
        for (int c = 0; c <= cellCount; c++)
        {
            CellStart[c] = 0;
        }
        int high = enemies.HighWater;
        for (int i = 0; i < high; i++)
        {
            if (!enemies.Alive[i])
            {
                _cellOf[i] = -1;
                continue;
            }
            int cell = CellCoord(enemies.Y[i]) * Cells + CellCoord(enemies.X[i]);
            _cellOf[i] = cell;
            CellStart[cell + 1]++;
        }
        for (int c = 0; c < cellCount; c++)
        {
            CellStart[c + 1] += CellStart[c];
            _cursor[c] = CellStart[c];
        }
        for (int i = 0; i < high; i++)
        {
            int cell = _cellOf[i];
            if (cell >= 0)
            {
                Items[_cursor[cell]++] = i;
            }
        }
    }
}
