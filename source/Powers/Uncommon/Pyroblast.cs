using KorzUtils.Helper;
using Modding.Utils;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.UnityComponents;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class Pyroblast : Power
{
    public override string Name => "Pyroblast";

    public override string Description => $"Vengeful Spirit ignites hit enemies causing <color={BurnEffect.TextColor}>burn</color>.";

    public override (float, float, float) BonusRates => new(0f, 40f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    protected override void Enable() => On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter += TakeDamage_OnEnter;
    
    protected override void Disable() => On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter -= TakeDamage_OnEnter;

    private void TakeDamage_OnEnter(On.HutongGames.PlayMaker.Actions.TakeDamage.orig_OnEnter orig, HutongGames.PlayMaker.Actions.TakeDamage self)
    {
        orig(self);
        if (self.IsCorrectContext("damages_enemy", null, "Send Event") && (self.Fsm.GameObject.name.Contains("Fireball")))
            if (self.Target.Value.GetComponent<HealthManager>()?.isDead == false)
                self.Target.Value.GetOrAddComponent<BurnEffect>().AddDamage(self.DamageDealt.Value / 2 + 5 + CombatController.SpiritLevel);
    }
}