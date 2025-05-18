using KorzUtils.Helper;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class DeepBreath : Power
{
    public override string Name => "Deep Breath";

    public override string Description => "Entering a new rom restores a bit of soul. Gained amount decreases with missing health.";

    public override (float, float, float) BonusRates => new(0f, 10f, 0f);

    internal override void Enable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        float soulToRestore = 5 + CombatController.SpiritLevel;
        float missingHealth = PDHelper.MaxHealth / PDHelper.Health;
        soulToRestore *= missingHealth;
        HeroController.instance.AddMPCharge(Mathf.CeilToInt(soulToRestore));
    }

    internal override void Disable() 
        => UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
}
