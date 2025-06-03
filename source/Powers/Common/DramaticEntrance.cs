using System.Reflection;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;

namespace TrialOfCrusaders.Powers.Common;

internal class DramaticEntrance : Power
{
    private MethodInfo _takeDamage;

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    protected override void Enable()
    {
        _takeDamage = typeof(HealthManager).GetMethod("TakeDamage", BindingFlags.Instance | BindingFlags.NonPublic);
        On.HeroController.FinishedEnteringScene += HeroController_FinishedEnteringScene;
    }

    protected override void Disable() => On.HeroController.FinishedEnteringScene -= HeroController_FinishedEnteringScene;

    private void HeroController_FinishedEnteringScene(On.HeroController.orig_FinishedEnteringScene orig, HeroController self, bool setHazardMarker, bool preventRunBob)
    {
        orig(self, setHazardMarker, preventRunBob);
        HitInstance hitInstance = new()
        {
            AttackType = AttackTypes.Generic,
            Source = HeroController.instance?.gameObject,
            Multiplier = 1,
            IsExtraDamage = true,
            DamageDealt = 10 + CombatController.CombatLevel * 2
        };
        foreach (HealthManager enemy in CombatController.Enemies)
        {
            if (enemy != null)
                _takeDamage.Invoke(enemy, [hitInstance]);
        }
    }
}