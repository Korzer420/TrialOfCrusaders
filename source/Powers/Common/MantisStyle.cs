using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class MantisStyle : Power
{
    private bool _parried;

    public override string Name => "Mantis Style";

    public override string Description => "An attack after a parry does increased damage.";

    public override (float, float, float) BonusRates => new(9f, 0f, 1f);

    internal override void Enable()
    {
        On.HeroController.NailParry += HeroController_NailParry;
        On.HeroController.CycloneInvuln += HeroController_CycloneInvuln;
        On.HealthManager.TakeDamage += HealthManager_TakeDamage;
    }

    internal override void Disable()
    {
        On.HeroController.NailParry -= HeroController_NailParry;
        On.HeroController.CycloneInvuln -= HeroController_CycloneInvuln;
        On.HealthManager.TakeDamage -= HealthManager_TakeDamage;
    }

    private void HeroController_CycloneInvuln(On.HeroController.orig_CycloneInvuln orig, HeroController self)
    {
        orig(self);
        _parried = true;
    }

    private void HeroController_NailParry(On.HeroController.orig_NailParry orig, HeroController self)
    {
        orig(self);
        _parried = true;
    }

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        if (hitInstance.AttackType == AttackTypes.Nail && _parried)
        {
            hitInstance.DamageDealt += 1000;
            _parried = false;
        }
        orig(self, hitInstance);
    }
}