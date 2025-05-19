using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Rare;

internal class DreamWalker : Power
{
    public override string Name => "Dream Walker";

    public override string Description => "Dream Nail can be casted while moving.";

    public override (float, float, float) BonusRates => new(30f, 30f, 40f);

    public override Rarity Tier => Rarity.Rare;

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
