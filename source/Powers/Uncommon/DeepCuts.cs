using KorzUtils.Helper;
using Modding.Utils;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.UnityComponents;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class DeepCuts : Power
{
    public override string Name => "Deep Cuts";

    public override string Description => $"Nail hits apply <color={BleedEffect.TextColor}>wounds</color>";

    public override (float, float, float) BonusRates => new(40f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    internal override void Enable() => On.HealthManager.TakeDamage += HealthManager_TakeDamage;

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        if (hitInstance.AttackType == AttackTypes.Nail && UnityEngine.Random.Range(0, 4) == 0)
            self.gameObject.AddComponent<BleedEffect>();
        orig(self, hitInstance);
    }

    internal override void Disable() => On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter -= TakeDamage_OnEnter;

    private void TakeDamage_OnEnter(On.HutongGames.PlayMaker.Actions.TakeDamage.orig_OnEnter orig, HutongGames.PlayMaker.Actions.TakeDamage self)
    {
        orig(self);
        if (self.IsCorrectContext("damages_enemy", null, "Send Event") && (self.Fsm.GameObject.name.Contains("Fireball")))
            if (self.Target.Value.GetComponent<HealthManager>()?.isDead == false)
                self.Target.Value.GetOrAddComponent<BurnEffect>().AddDamage(self.DamageDealt.Value / 2 + 5 + CombatController.SpiritLevel);
    }
}
