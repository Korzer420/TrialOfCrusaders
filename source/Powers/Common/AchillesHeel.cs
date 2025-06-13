using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Common;

internal class AchillesHeel : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    public override bool CanAppear => !CombatController.HasPower<CaringShell>(out _);

    public override StatScaling Scaling => StatScaling.Endurance;

    // Spell Twister room: Block bottom entrance
    // Grimmchild wrong level
    // Dramatic entrance causes Aspid arena softlock. -> Rework to apply weakened
    // Beast den doesn't reset.
    // Lifeblood being weird.
    // Prevent dive into transition.
}