using KorzUtils.Enums;
using KorzUtils.Helper;
using System;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Rare;

internal class ImprovedFocus : Power
{
    public override (float, float, float) BonusRates => new(0f, 20f, 80f);

    public override Rarity Tier => Rarity.Rare;

    protected override void Enable() => On.HutongGames.PlayMaker.Actions.Tk2dPlayFrame.OnEnter += Tk2dPlayFrame_OnEnter;
    
    protected override void Disable() => On.HutongGames.PlayMaker.Actions.Tk2dPlayFrame.OnEnter -= Tk2dPlayFrame_OnEnter;

    private void Tk2dPlayFrame_OnEnter(On.HutongGames.PlayMaker.Actions.Tk2dPlayFrame.orig_OnEnter orig, HutongGames.PlayMaker.Actions.Tk2dPlayFrame self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Focus Heal"))
        {
            int leftoverSoul = PDHelper.MPCharge + PDHelper.MPReserve;
            int healAmount = (int)Math.Floor((float)leftoverSoul / 33);
            HeroController.instance.TakeMP(healAmount * 33);
            if (CharmHelper.EquippedCharm(CharmRef.DeepFocus))
                healAmount *= 2;
            self.Fsm.Variables.FindFsmInt("Health Increase").Value += healAmount;
        }
        orig(self);
    }
}
