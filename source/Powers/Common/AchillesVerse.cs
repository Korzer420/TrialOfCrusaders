using TrialOfCrusaders.Controller;

namespace TrialOfCrusaders.Powers.Common;

internal class AchillesVerse : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    public override bool CanAppear => !CombatController.HasPower<CaringShell>(out _);
}