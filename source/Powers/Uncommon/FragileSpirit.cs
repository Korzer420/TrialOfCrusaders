using HutongGames.PlayMaker.Actions;
using KorzUtils.Helper;
using Modding.Utils;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.UnityComponents;

namespace TrialOfCrusaders.Powers.Uncommon;

public class FragileSpirit : Power
{
    public override string Name => "Fragile Spirit";

    public override string Description => $"All spells cause <color={BurnEffect.TextColor}>burn</color>. All <color={BurnEffect.TextColor}>burn</color> damage is doubled. No longer works upon taking damage.";

    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public bool SpiritActive { get; set; } = true;

    internal override void Enable()
    {
        BurnEffect.PoweredUp = SpiritActive;
        On.HeroController.TakeDamage += HeroController_TakeDamage;
        On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter += TakeDamage_OnEnter;
    }

    internal override void Disable()
    {
        BurnEffect.PoweredUp = false;
        On.HeroController.TakeDamage -= HeroController_TakeDamage;
        On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter -= TakeDamage_OnEnter;
    }

    private void HeroController_TakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, UnityEngine.GameObject go, GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
    {
        orig(self, go, damageSide, damageAmount, hazardType);
        if (damageAmount != 0 && SpiritActive)
        {
            SpiritActive = false;
            HeroController.instance.proxyFSM.GetState("Flower?").GetFirstAction<ActivateGameObject>().gameObject.GameObject.Value.SetActive(true);
            // To do: Remove progress after last save point (i.e taking damage on floor 89 should ignore progress after 85 to avoid abuse.
            GameManager.instance.SaveGame();
        }
    }

    private void TakeDamage_OnEnter(On.HutongGames.PlayMaker.Actions.TakeDamage.orig_OnEnter orig, HutongGames.PlayMaker.Actions.TakeDamage self)
    {
        orig(self);
        if (SpiritActive && self.IsCorrectContext("damages_enemy", null, "Send Event") && self.AttackType.Value == 2)
            if (self.Target.Value.GetComponent<HealthManager>()?.isDead == false)
                self.Target.Value.GetOrAddComponent<BurnEffect>().AddDamage(self.DamageDealt.Value / 2 + 5 + CombatController.SpiritLevel);
    }
}
