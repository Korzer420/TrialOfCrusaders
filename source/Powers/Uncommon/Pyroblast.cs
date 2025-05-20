using KorzUtils.Helper;
using Modding.Utils;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.UnityComponents;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class Pyroblast : Power
{
    public override (float, float, float) BonusRates => new(0f, 40f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    protected override void Enable() => On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter += TakeDamage_OnEnter;
    
    protected override void Disable() => On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter -= TakeDamage_OnEnter;

    private void TakeDamage_OnEnter(On.HutongGames.PlayMaker.Actions.TakeDamage.orig_OnEnter orig, HutongGames.PlayMaker.Actions.TakeDamage self)
    {
        orig(self);
        if (self.IsCorrectContext("damages_enemy", null, "Send Event") && (self.Fsm.GameObject.name.Contains("Fireball")))
            if (self.Target.Value.GetComponent<HealthManager>()?.isDead == false && RngProvider.GetRandom(0, 20) <= CombatController.SpiritLevel)
                self.Target.Value.GetOrAddComponent<BurnEffect>().AddDamage(self.DamageDealt.Value / 2 + 5 + CombatController.SpiritLevel);
    }
}