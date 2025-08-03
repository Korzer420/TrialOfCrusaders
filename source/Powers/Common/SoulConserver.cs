using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Common;

internal class SoulConserver : Power
{
    public override (float, float, float) BonusRates => new(0f, 10f, 0f);

    public override StatScaling Scaling => StatScaling.Spirit;

    public override DraftPool Pools => DraftPool.Spirit;

    protected override void Enable()
    {
        On.HutongGames.PlayMaker.Actions.SetVelocity2d.OnEnter += SetVelocity2d_OnEnter;
        On.HutongGames.PlayMaker.Actions.Tk2dPlayFrame.OnEnter += Tk2dPlayFrame_OnEnter;
    }

    protected override void Disable()
    { 
        On.HutongGames.PlayMaker.Actions.SetVelocity2d.OnEnter -= SetVelocity2d_OnEnter;
        On.HutongGames.PlayMaker.Actions.Tk2dPlayFrame.OnEnter -= Tk2dPlayFrame_OnEnter;
    }

    private void Tk2dPlayFrame_OnEnter(On.HutongGames.PlayMaker.Actions.Tk2dPlayFrame.orig_OnEnter orig, HutongGames.PlayMaker.Actions.Tk2dPlayFrame self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Focus Heal*") && UnityEngine.Random.Range(0, 80) < CombatRef.SpiritLevel)
        {
            // Check to make it work with ImprovedFocus
            int amount = self.Fsm.Variables.FindFsmInt("Health Increase").Value;
            if (CharmHelper.EquippedCharm(CharmRef.DeepFocus))
                amount /= 2;
            HeroController.instance.AddMPCharge(amount * 33);
        }
        orig(self);
    }

    private void SetVelocity2d_OnEnter(On.HutongGames.PlayMaker.Actions.SetVelocity2d.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetVelocity2d self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Spell End") && UnityEngine.Random.Range(0, 80) < CombatRef.SpiritLevel)
            HeroController.instance.AddMPCharge(self.Fsm.Variables.FindFsmInt("MP Cost").Value);
        orig(self);
    }
}
