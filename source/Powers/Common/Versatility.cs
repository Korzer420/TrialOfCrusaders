using KorzUtils.Helper;
using Modding;

namespace TrialOfCrusaders.Powers.Common;

internal class Versatility : Power
{
    public bool CastedSpell { get; set; } = false;

    public override string Name => "Versatility";

    public override string Description => "The first nail hit after casting a spell grants bonus soul.";

    public override (float, float, float) BonusRates => new(5f, 5f, 0f);

    protected override void Enable()
    {
        CastedSpell = false;
        On.HutongGames.PlayMaker.Actions.SetVelocity2d.OnEnter += SetVelocity2d_OnEnter;
        ModHooks.SoulGainHook += ModHooks_SoulGainHook;
    }

    protected override void Disable()
    {
        On.HutongGames.PlayMaker.Actions.SetVelocity2d.OnEnter -= SetVelocity2d_OnEnter;
        ModHooks.SoulGainHook -= ModHooks_SoulGainHook;
    }

    private void SetVelocity2d_OnEnter(On.HutongGames.PlayMaker.Actions.SetVelocity2d.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetVelocity2d self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Spell End"))
            CastedSpell = true;
        orig(self);
    }

    private int ModHooks_SoulGainHook(int amount)
    {
        if (CastedSpell)
            amount += 2 + (CombatController.SpiritLevel + CombatController.CombatLevel) / 8;
        CastedSpell = false;
        return amount;
    }
}