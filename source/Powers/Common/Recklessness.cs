using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class Recklessness : Power
{
    public override string Name => "Recklessness";

    public override string Description => "Great Slash and Dash Slash deal 400% increased damage, but you take damage if this doesn't kill the enemy.";

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    internal override void Enable() => On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter += TakeDamage_OnEnter;

    private void TakeDamage_OnEnter(On.HutongGames.PlayMaker.Actions.TakeDamage.orig_OnEnter orig, HutongGames.PlayMaker.Actions.TakeDamage self)
    {
        bool isNailArt = self.IsCorrectContext("damages_enemy", null, "Send Event") && (self.Fsm.GameObject.name == "Great Slash" || self.Fsm.GameObject.name == "Dash Slash");
        if (isNailArt)
            self.DamageDealt.Value *= 5;
        orig(self);
        if (isNailArt)
        {
            HealthManager enemy = self.Target.Value.GetComponent<HealthManager>() ?? self.Target.Value.GetComponentInChildren<HealthManager>();
            if (enemy?.isDead == false)
                HeroController.instance.TakeDamage(self.Target.Value, GlobalEnums.CollisionSide.top, 1, 1);
        }
    }

    internal override void Disable() => On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter -= TakeDamage_OnEnter;
}
