using TrialOfCrusaders.Controller;
using TrialOfCrusaders.UnityComponents;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class RoyalDecree : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    protected override void Enable() => On.HeroController.FinishedEnteringScene += HeroController_FinishedEnteringScene;

    protected override void Disable() => On.HeroController.FinishedEnteringScene -= HeroController_FinishedEnteringScene;

    private void HeroController_FinishedEnteringScene(On.HeroController.orig_FinishedEnteringScene orig, HeroController self, bool setHazardMarker, bool preventRunBob)
    {
        if (!StageController.CurrentRoom.BossRoom)
        {
            GameObject royalMark = new("Royal Decree");
            royalMark.SetActive(false);
            royalMark.AddComponent<RoyalMark>().AttachedEnemy = CombatController.Enemies[UnityEngine.Random.Range(0, CombatController.Enemies.Count)];
            royalMark.SetActive(true);
        }
        orig(self, setHazardMarker, preventRunBob);
    }
}