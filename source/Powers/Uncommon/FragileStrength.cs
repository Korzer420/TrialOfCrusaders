using HutongGames.PlayMaker.Actions;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class FragileStrength : Power
{
    public override string Name => "Fragile Strength";

    public override string Description => $"Nail damage is increased by 300%. No longer works upon taking damage.";

    public override (float, float, float) BonusRates => new(40f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public bool StrengthActive { get; set; } = true;

    protected override void Enable()
    {
        On.HealthManager.TakeDamage += HealthManager_TakeDamage;
        On.HeroController.TakeDamage += HeroController_TakeDamage;
    }

    protected override void Disable()
    {
        On.HealthManager.TakeDamage -= HealthManager_TakeDamage;
        On.HeroController.TakeDamage -= HeroController_TakeDamage;
    }

    private void HeroController_TakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, UnityEngine.GameObject go, GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
    {
        orig(self, go, damageSide, damageAmount, hazardType);
        if (damageAmount != 0 && StrengthActive)
        {
            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
            StrengthActive = false;
            HeroController.instance.proxyFSM.GetState("Flower?").GetFirstAction<ActivateGameObject>().gameObject.GameObject.Value.SetActive(true);
            // To do: Remove progress after last save point (i.e taking damage on floor 89 should ignore progress after 85 to avoid abuse.
            GameManager.instance.SaveGame();
        }
    }

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        if (hitInstance.AttackType == AttackTypes.Nail && StrengthActive)
            hitInstance.DamageDealt *= 4;
        orig(self, hitInstance);
    }
}