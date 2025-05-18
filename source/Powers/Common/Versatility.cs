using KorzUtils.Helper;
using Modding;
using System;
using TrialOfCrusaders.UnityComponents;

namespace TrialOfCrusaders.Powers.Common;

internal class Versatility : Power
{
    private bool _castedSpell = false;

    public override string Name => "Versatility";

    public override string Description => "The first nail hit after casting a spell grants bonus soul.";

    public override (float, float, float) BonusRates => new(5f, 5f, 0f);

    internal override void Enable()
    {
        _castedSpell = false;
        On.HutongGames.PlayMaker.Actions.SetVelocity2d.OnEnter += SetVelocity2d_OnEnter;
        ModHooks.SoulGainHook += ModHooks_SoulGainHook;
    }

    internal override void Disable()
    {
        On.HutongGames.PlayMaker.Actions.SetVelocity2d.OnEnter -= SetVelocity2d_OnEnter;
        ModHooks.SoulGainHook -= ModHooks_SoulGainHook;
    }

    private void SetVelocity2d_OnEnter(On.HutongGames.PlayMaker.Actions.SetVelocity2d.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetVelocity2d self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Spell End"))
            _castedSpell = true;
        orig(self);
    }

    private int ModHooks_SoulGainHook(int amount)
    {
        if (_castedSpell)
            amount += 2 + (CombatController.SpiritLevel + CombatController.CombatLevel) / 8;
        _castedSpell = false;
        return amount;
    }
}