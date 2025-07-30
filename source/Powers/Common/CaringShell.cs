using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Common;

internal class CaringShell : Power
{
    public override DraftPool Pools => DraftPool.Endurance;

    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    public override bool CanAppear => !PowerRef.HasPower<AchillesHeel>(out _) && !HasPower<BrittleShell>();

    public override StatScaling Scaling => StatScaling.Endurance;
}