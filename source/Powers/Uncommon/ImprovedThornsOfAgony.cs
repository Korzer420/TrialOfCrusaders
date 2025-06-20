using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ImprovedThornsOfAgony : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 40f);

    public override Rarity Tier => Rarity.Uncommon;

    public override DraftPool Pools => DraftPool.Charm | DraftPool.Combat | DraftPool.Upgrade;

    public override bool CanAppear => HasPower<ThornsOfAgony>();

    protected override void Enable()
    {
        CharmHelper.EnsureEquipCharm(KorzUtils.Enums.CharmRef.ThornsOfAgony);
        On.HutongGames.PlayMaker.Actions.SetPosition.OnEnter += SetPosition_OnEnter;
        On.HutongGames.PlayMaker.Actions.SendMessage.OnEnter += SendMessage_OnEnter;
    }

    protected override void Disable()
    {
        On.HutongGames.PlayMaker.Actions.SetPosition.OnEnter -= SetPosition_OnEnter;
        On.HutongGames.PlayMaker.Actions.SendMessage.OnEnter -= SendMessage_OnEnter;
    }

    private void SendMessage_OnEnter(On.HutongGames.PlayMaker.Actions.SendMessage.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SendMessage self)
    {
        // Skip the action without disabling it outright.
        if (self.IsCorrectContext("Thorn Counter", "Counter Start", null) 
            && self.functionCall.FunctionName == "AffectedByGravity" || self.functionCall.FunctionName == "RelinquishControl")
            self.Finish();
        else
            orig(self);
    }

    private void SetPosition_OnEnter(On.HutongGames.PlayMaker.Actions.SetPosition.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetPosition self)
    {
        // Skip the action without disabling it outright.
        if (self.IsCorrectContext("Thorn Counter", null, null) && (self.State.Name == "Counter Start" || self.State.Name == "Counter"))
            self.Finish();
        else
            orig(self);
    }
}
