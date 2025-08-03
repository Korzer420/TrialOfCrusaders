using System;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Common;

internal class ChannelledEnergy : Power
{
    public override DraftPool Pools => DraftPool.Endurance | DraftPool.Instant;
    
    public override bool CanAppear => !PowerRef.HasPower<InUtterDarkness>(out _) && (CombatRef.CombatLevel + CombatRef.SpiritLevel + CombatRef.EnduranceLevel) > 0;

    public bool Activated { get; set; }

    public override string Description => $"Grant {Math.Max(Math.Max(CombatRef.EnduranceLevel, CombatRef.CombatLevel), CombatRef.SpiritLevel)} lifeblood. Scales with your highest stat.";

    public override (float, float, float) BonusRates => new(3f, 3f, 4f);

    protected override void Enable()
    {
        if (!Activated)
        {
            int highestStat = Math.Max(Math.Max(CombatRef.EnduranceLevel, CombatRef.CombatLevel), CombatRef.SpiritLevel);
            for (int i = 0; i < highestStat; i++)
                EventRegister.SendEvent("ADD BLUE HEALTH");
        }
        Activated = true;
    }
}