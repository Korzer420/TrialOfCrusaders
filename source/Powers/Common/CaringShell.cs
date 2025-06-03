using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;

namespace TrialOfCrusaders.Powers.Common;

internal class CaringShell : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    public override bool CanAppear => !CombatController.HasPower<AchillesVerse>(out _);
}