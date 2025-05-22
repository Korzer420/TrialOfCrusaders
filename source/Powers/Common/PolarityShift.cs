using KorzUtils.Helper;
using TrialOfCrusaders.Controller;

namespace TrialOfCrusaders.Powers.Common;

internal class PolarityShift : Power
{
    public override string Name => "Polarity Shift";

    public override string Description => "Cast spells are sometimes the opposite level.";

    public override (float, float, float) BonusRates => new(0f, 10f, 0f);

    public override bool CanAppear => CombatController.HasSpell();

    protected override void Enable() => On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += IntCompare_OnEnter;

    private void IntCompare_OnEnter(On.HutongGames.PlayMaker.Actions.IntCompare.orig_OnEnter orig, HutongGames.PlayMaker.Actions.IntCompare self)
    {
        if (self.IsCorrectContext("Spell Control", null, null) && self.State.Name.StartsWith("Level Check"))
        {
            if (self.integer1.Value == 1 && UnityEngine.Random.Range(1, 41) <= CombatController.SpiritLevel + 1)
                self.integer1.Value = 2;
            else if (self.integer1.Value == 2 && UnityEngine.Random.Range(1, 41 - CombatController.SpiritLevel) >= 16)
                self.integer1.Value = 1;
        }
        orig(self);
        self.integer1.Value = 1;
        self.integer2.Value = 2;
    }

    protected override void Disable() => On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter -= IntCompare_OnEnter;
}