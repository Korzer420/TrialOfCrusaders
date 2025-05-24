using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Common;

internal class Desperation : Power
{
    public override bool CanAppear => !CombatController.HasPower<InUtterDarkness>(out _);

    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    protected override void Enable() => On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += PlayerDataBoolTest_OnEnter;

    protected override void Disable() => On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += PlayerDataBoolTest_OnEnter;

    private void PlayerDataBoolTest_OnEnter(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, HutongGames.PlayMaker.Actions.PlayerDataBoolTest self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Deep Focus Speed"))
            // The formular scale up to 3 times speed at exactly 1 health.
            self.Fsm.Variables.FindFsmFloat("Time Per MP Drain").Value /= 1 + (PDHelper.MaxHealth - PDHelper.Health) / (PDHelper.MaxHealth - 1) * 2;
        orig(self);
    }
}
