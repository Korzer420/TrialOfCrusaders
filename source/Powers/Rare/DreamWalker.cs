using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;

namespace TrialOfCrusaders.Powers.Rare;

internal class DreamWalker : Power
{
    public override (float, float, float) BonusRates => new(30f, 30f, 40f);

    public override Rarity Tier => Rarity.Rare;

    public override DraftPool Pools => DraftPool.Upgrade | DraftPool.Ability;

    public override bool CanAppear => PowerRef.HasPower<DreamNail>(out _);

    protected override void Enable()
    {
        On.HeroController.CanDreamNail += HeroController_CanDreamNail;
        On.HutongGames.PlayMaker.Actions.SendMessage.OnEnter += SendMessage_OnEnter;
    }

    protected override void Disable()
    {
        On.HeroController.CanDreamNail -= HeroController_CanDreamNail;
        On.HutongGames.PlayMaker.Actions.SendMessage.OnEnter -= SendMessage_OnEnter;
    }

    private void SendMessage_OnEnter(On.HutongGames.PlayMaker.Actions.SendMessage.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SendMessage self)
    {
        if (self.IsCorrectContext("Dream Nail", "Knight", "Take Control") && self.functionCall.FunctionName == "RelinquishControl")
            self.Finish();
        else
            orig(self);
    }

    private bool HeroController_CanDreamNail(On.HeroController.orig_CanDreamNail orig, HeroController self) => true;        
}
