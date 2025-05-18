using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class Desperation : Power
{
    public override string Name => "Desperation";

    public override string Description => "Excessive soul may manifest in a soul sphere. Only one sphere can be active at a time.";

    public override (float, float, float) BonusRates => new(0f, 10f, 0f);

    internal override void Enable()
    {
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += PlayerDataBoolTest_OnEnter;
    }

    private void PlayerDataBoolTest_OnEnter(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, HutongGames.PlayMaker.Actions.PlayerDataBoolTest self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Deep Focus Speed"))
            // The formular scale up to 3 times speed at exactly 1 health.
            self.Fsm.Variables.FindFsmFloat("Time Per MP Drain").Value /= 1 + (PDHelper.MaxHealth - PDHelper.Health) / (PDHelper.MaxHealth - 1) * 2;
        orig(self);
    }

    internal override void Disable()
    {

    }
}
