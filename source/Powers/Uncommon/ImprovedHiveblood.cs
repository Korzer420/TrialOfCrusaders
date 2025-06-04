using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ImprovedHiveblood : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 40f);

    public override Rarity Tier => Rarity.Uncommon;

    public override bool CanAppear => HasPower<Hiveblood>() && !HasPower<InUtterDarkness>();

    protected override void Enable()
    {
        CharmHelper.EnsureEquipCharm(KorzUtils.Enums.CharmRef.Hiveblood);
        On.HutongGames.PlayMaker.Actions.FloatAdd.OnEnter += FloatAdd_OnEnter;
    }

    protected override void Disable() => On.HutongGames.PlayMaker.Actions.FloatAdd.OnEnter -= FloatAdd_OnEnter;

    private void FloatAdd_OnEnter(On.HutongGames.PlayMaker.Actions.FloatAdd.orig_OnEnter orig, HutongGames.PlayMaker.Actions.FloatAdd self)
    {
        if (self.IsCorrectContext("Hive Health Regen", "Health", "Recover 1") || self.IsCorrectContext("Hive Health Regen", "Health", "Recover 2"))
        {
            float vanillaValue = self.floatVariable.Value;
            self.floatVariable.Value = 2f;
            orig(self);
            self.floatVariable.Value = vanillaValue;
        }
        else
            orig(self);
    }
}
