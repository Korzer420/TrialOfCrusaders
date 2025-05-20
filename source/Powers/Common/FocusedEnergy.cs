using System;
using TrialOfCrusaders.Powers.Uncommon;

namespace TrialOfCrusaders.Powers.Common;

internal class FocusedEnergy : Power
{
    public override bool CanAppear => !CombatController.HasPower<InUtterDarkness>(out _);

    public bool Activated { get; set; }

    public override string Description => $"Grant {Math.Max(Math.Max(CombatController.EnduranceLevel, CombatController.CombatLevel), CombatController.SpiritLevel)} lifeblood.";

    public override (float, float, float) BonusRates => new(3f, 3f, 4f);

    protected override void Enable()
    {
        if (!Activated)
        {
            int highestStat = Math.Max(Math.Max(CombatController.EnduranceLevel, CombatController.CombatLevel), CombatController.SpiritLevel);
            for (int i = 0; i < highestStat; i++)
                EventRegister.SendEvent("ADD BLUE HEALTH");
        }
        Activated = true;
    }
}