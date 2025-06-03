using System;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Common;

internal class FocusedEnergy : Power
{
    public override bool CanAppear => !CombatController.HasPower<InUtterDarkness>(out _) && (CombatController.CombatLevel + CombatController.SpiritLevel + CombatController.EnduranceLevel) > 0;

    public bool Activated { get; set; }

    public override string Description => $"Grant {Math.Max(Math.Max(CombatController.EnduranceLevel, CombatController.CombatLevel), CombatController.SpiritLevel)} lifeblood. Scales with your highest stat.";

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