using KorzUtils.Helper;
using System;

namespace TrialOfCrusaders.Powers.Common;

internal class FocusedEnergy : Power
{
    public override string Name => "Focused energy.";

    public override string Description => $"Grant {Math.Max(Math.Max(CombatController.EnduranceLevel, CombatController.CombatLevel), CombatController.SpiritLevel)} lifeblood.";

    public override (float, float, float) BonusRates => new(3f, 3f, 4f);

    protected override void Enable()
    {
        int highestStat = Math.Max(Math.Max(CombatController.EnduranceLevel, CombatController.CombatLevel), CombatController.SpiritLevel);
        for (int i = 0; i < highestStat; i++)
            EventRegister.SendEvent("ADD BLUE HEALTH");
    }

    protected override void Disable()
    {
        PDHelper.HasUpwardSlash = false;
        PDHelper.HasNailArt = false;
    }
}