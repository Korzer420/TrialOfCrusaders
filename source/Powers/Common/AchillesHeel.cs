using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Common;

internal class AchillesHeel : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    public override bool CanAppear => !CombatController.HasPower<CaringShell>(out _);

    public override StatScaling Scaling => StatScaling.Endurance;

    public override DraftPool Pools => DraftPool.Endurance | DraftPool.Risk;
}