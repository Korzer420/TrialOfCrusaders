using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Common;

internal class AchillesHeel : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    public override bool CanAppear => !PowerRef.HasPower<CaringShell>(out _) && !HasPower<BrittleShell>();

    public override StatScaling Scaling => StatScaling.Endurance;

    public override DraftPool Pools => DraftPool.Endurance | DraftPool.Risk;
}