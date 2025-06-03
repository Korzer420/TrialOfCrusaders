using KorzUtils.Helper;
using TrialOfCrusaders.Data;

namespace TrialOfCrusaders.Powers.Common;

internal class Versatility : Power
{
    public bool CastedSpell { get; set; } = false;

    public override (float, float, float) BonusRates => new(5f, 5f, 0f);

    protected override void Enable()
    {
        CastedSpell = false;
        On.HutongGames.PlayMaker.Actions.SetVelocity2d.OnEnter += SetVelocity2d_OnEnter;
    }

    protected override void Disable() => On.HutongGames.PlayMaker.Actions.SetVelocity2d.OnEnter -= SetVelocity2d_OnEnter;

    private void SetVelocity2d_OnEnter(On.HutongGames.PlayMaker.Actions.SetVelocity2d.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetVelocity2d self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Spell End"))
            CastedSpell = true;
        orig(self);
    }
}