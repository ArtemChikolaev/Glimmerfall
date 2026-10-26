using Glimmerfall.Sim.Determinism;
using Glimmerfall.Sim.Numerics;

namespace Glimmerfall.Sim.Core;

/// <summary>
/// Отряд: три героя, на поле — один активный, двое в резерве (PRD §5). Позиция общая: свап меняет героя,
/// а не место. Свап и рывок — ввод; смерть активного — вынужденный свап (скилл sim-invariants, правило 7).
/// </summary>
public sealed class Squad
{
    public const int Size = 3;

    public readonly int[] Kind = new int[Size];
    public readonly int[] Hp = new int[Size];
    public readonly int[] MaxHp = new int[Size];
    public readonly bool[] Alive = new bool[Size];
    public readonly int[] AttackCooldown = new int[Size];

    public int Active;
    public Fix X;
    public Fix Y;
    public Fix PrevX;
    public Fix PrevY;
    /// <summary>Последнее ненулевое направление движения, единичный вектор.</summary>
    public Fix FacingX = Fix.One;
    public Fix FacingY = Fix.Zero;

    public int DashCooldown;
    public int DashTicks;
    public Fix DashDirX;
    public Fix DashDirY;
    public int SwapCooldown;
    public int AssistCooldown;
    /// <summary>Окно связки после ассиста (тики) и слот выпрыгнувшего героя; свап на него в окне — дуэт-приём.</summary>
    public int LinkTicks;
    public int LinkHelper = -1;
    public int InvulnerableTicks;
    /// <summary>Поворот кольца «Новы» и спирали спайка орды, единицы угла.</summary>
    public int SpinAngle;

    public int AliveCount
    {
        get
        {
            int n = 0;
            for (int i = 0; i < Size; i++)
            {
                if (Alive[i])
                {
                    n++;
                }
            }
            return n;
        }
    }

    public void AddTo(ref StateHasher hasher)
    {
        for (int i = 0; i < Size; i++)
        {
            hasher.Add(Kind[i]);
            hasher.Add(Hp[i]);
            hasher.Add(MaxHp[i]);
            hasher.Add(Alive[i]);
            hasher.Add(AttackCooldown[i]);
        }
        hasher.Add(Active);
        hasher.Add(X.Raw);
        hasher.Add(Y.Raw);
        hasher.Add(FacingX.Raw);
        hasher.Add(FacingY.Raw);
        hasher.Add(DashCooldown);
        hasher.Add(DashTicks);
        hasher.Add(DashDirX.Raw);
        hasher.Add(DashDirY.Raw);
        hasher.Add(SwapCooldown);
        hasher.Add(AssistCooldown);
        hasher.Add(LinkTicks);
        hasher.Add(LinkHelper);
        hasher.Add(InvulnerableTicks);
        hasher.Add(SpinAngle);
    }
}
