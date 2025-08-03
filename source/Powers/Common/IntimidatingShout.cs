using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;
using TrialOfCrusaders.UnityComponents.Debuffs;

namespace TrialOfCrusaders.Powers.Common;

internal class IntimidatingShout : Power
{
    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    public override StatScaling Scaling => StatScaling.Combat;

    public override DraftPool Pools => DraftPool.Burst | DraftPool.Endurance | DraftPool.Debuff;

    protected override void Enable() => On.HeroController.FinishedEnteringScene += HeroController_FinishedEnteringScene;

    protected override void Disable() => On.HeroController.FinishedEnteringScene -= HeroController_FinishedEnteringScene;

    private void HeroController_FinishedEnteringScene(On.HeroController.orig_FinishedEnteringScene orig, HeroController self, bool setHazardMarker, bool preventRunBob)
    {
        orig(self, setHazardMarker, preventRunBob);
        try
        {
            for (int i = 0; i < CombatRef.ActiveEnemies.Count; i++)
                if (CombatRef.ActiveEnemies[i] != null && CombatRef.ActiveEnemies[i].gameObject.activeSelf)
                    CombatRef.ActiveEnemies[i].gameObject.AddComponent<WeakenedEffect>().Timer = 5 + CombatRef.CombatLevel;
        }
        catch (System.Exception ex)
        {
            LogManager.Log("Failed to setup weakened effect. ", ex);
        }
    }
}