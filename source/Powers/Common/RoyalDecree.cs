using TrialOfCrusaders.UnityComponents;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class RoyalDecree : Power
{
    public override string Name => "Royal decree";

    public override string Description => "Killing all enemies in the marked order will award bonus geo and sometimes power orbs.";

    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    protected override void Enable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        GameObject royalMark = new("Royal Decree");
        royalMark.SetActive(false);
        royalMark.AddComponent<RoyalMark>().AttachedEnemy = CombatController.Enemies[UnityEngine.Random.Range(0, CombatController.Enemies.Count)];
        royalMark.SetActive(true);
    }

    protected override void Disable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
    }
}