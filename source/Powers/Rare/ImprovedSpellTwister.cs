using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Rare;

internal class ImprovedSpellTwister : Power
{
    public override string Name => "Improved Spell Twister";

    public override string Description => "Reduces spell cost even further.";

    public override (float, float, float) BonusRates => new(0f, 100f, 0f);

    public override Rarity Tier => Rarity.Rare;

    internal override void Enable()
    {
        On.HutongGames.PlayMaker.Actions.SetFsmInt.OnEnter += SetFsmInt_OnEnter;
        CoroutineHelper.WaitForHero(() =>
        {
            HeroController.instance.spellControl.FsmVariables.FindFsmInt("MP Cost").Value = 16;
        }, true);
    }

    internal override void Disable() => On.HutongGames.PlayMaker.Actions.SetFsmInt.OnEnter -= SetFsmInt_OnEnter;

    private void SetFsmInt_OnEnter(On.HutongGames.PlayMaker.Actions.SetFsmInt.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetFsmInt self)
    {
        orig(self);
        if (self.IsCorrectContext("Set Spell Cost", "Charm Effects", "Mage"))
            HeroController.instance.spellControl.FsmVariables.FindFsmInt("MP Cost").Value = 16;
    }
}
