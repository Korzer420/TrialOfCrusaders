using HutongGames.PlayMaker.Actions;
using KorzUtils.Helper;
using Modding.Utils;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;
using TrialOfCrusaders.UnityComponents;

namespace TrialOfCrusaders.Powers.Uncommon;

public class FragileSpirit : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public bool SpiritActive { get; set; } = true;

    public override bool CanAppear => !HasPower<PaleShell>() && CombatController.HasSpell();

    protected override void Enable()
    {
        CombatController.TookDamage += CombatController_TookDamage;
        On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter += TakeDamage_OnEnter;
    }

    protected override void Disable()
    {
        CombatController.TookDamage -= CombatController_TookDamage;
        On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter -= TakeDamage_OnEnter;
    }

    private void CombatController_TookDamage() => SpiritActive = false;

    private void TakeDamage_OnEnter(On.HutongGames.PlayMaker.Actions.TakeDamage.orig_OnEnter orig, HutongGames.PlayMaker.Actions.TakeDamage self)
    {
        orig(self);
        if (SpiritActive && self.IsCorrectContext("damages_enemy", null, "Send Event") && self.AttackType.Value == 2)
            if (self.Target.Value.GetComponent<HealthManager>()?.isDead == false)
                self.Target.Value.GetOrAddComponent<BurnEffect>().AddDamage(self.DamageDealt.Value / 2 + 5 + CombatController.SpiritLevel);
    }
}
