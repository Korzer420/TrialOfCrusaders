using System.Reflection;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;
using TrialOfCrusaders.UnityComponents.Debuffs;

namespace TrialOfCrusaders.Powers.Common;

internal class IntimidatingShout : Power
{
    private MethodInfo _takeDamage;

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    public override StatScaling Scaling => StatScaling.Combat;

    protected override void Enable()
    {
        _takeDamage = typeof(HealthManager).GetMethod("TakeDamage", BindingFlags.Instance | BindingFlags.NonPublic);
        On.HeroController.FinishedEnteringScene += HeroController_FinishedEnteringScene;
    }

    protected override void Disable() => On.HeroController.FinishedEnteringScene -= HeroController_FinishedEnteringScene;

    private void HeroController_FinishedEnteringScene(On.HeroController.orig_FinishedEnteringScene orig, HeroController self, bool setHazardMarker, bool preventRunBob)
    {
        orig(self, setHazardMarker, preventRunBob);
        try
        {
            for (int i = 0; i < CombatController.ActiveEnemies.Count; i++)
                if (CombatController.ActiveEnemies[i] != null && CombatController.ActiveEnemies[i].gameObject.activeSelf)
                    CombatController.ActiveEnemies[i].gameObject.AddComponent<WeakenedEffect>().Timer = 5 + CombatController.CombatLevel;
        }
        catch (System.Exception ex)
        {
            LogManager.Log("Failed to setup weakened effect. ", ex);
        }
    }
}