using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Common;

internal class CaringShell : Power
{
    public override DraftPool Pools => DraftPool.Endurance;

    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    public override bool CanAppear => !CombatController.HasPower<AchillesHeel>(out _);

    public override StatScaling Scaling => StatScaling.Endurance;
}