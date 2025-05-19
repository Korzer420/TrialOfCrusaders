using HutongGames.PlayMaker.Actions;
using KorzUtils.Helper;
using Modding;
using Modding.Utils;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.UnityComponents;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class FragileGreed : Power
{
    public override string Name => "Fragile Greed";

    public override string Description => $"Geo is 200% more worth. No longer works upon taking damage.";

    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public bool GreedActive { get; set; } = true;

    protected override void Enable()
    {
        On.HeroController.AddGeo += HeroController_AddGeo;
        On.HeroController.TakeDamage += HeroController_TakeDamage;
    }

    protected override void Disable()
    {
        On.HeroController.AddGeo -= HeroController_AddGeo;
        On.HeroController.TakeDamage -= HeroController_TakeDamage;
    }

    private void HeroController_TakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, UnityEngine.GameObject go, GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
    {
        orig(self, go, damageSide, damageAmount, hazardType);
        if (damageAmount != 0 && GreedActive)
        {
            GreedActive = false;
            HeroController.instance.proxyFSM.GetState("Flower?").GetFirstAction<ActivateGameObject>().gameObject.GameObject.Value.SetActive(true);
            // To do: Remove progress after last save point (i.e taking damage on floor 89 should ignore progress after 85 to avoid abuse.
            GameManager.instance.SaveGame();
        }
    }

    private void HeroController_AddGeo(On.HeroController.orig_AddGeo orig, HeroController self, int amount)
    {
        if (GreedActive)
            amount *= 3;
        orig(self, amount);
    }
}