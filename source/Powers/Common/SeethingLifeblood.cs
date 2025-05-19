using IL.InControl;
using KorzUtils.Helper;
using Modding.Utils;
using TrialOfCrusaders.UnityComponents;

namespace TrialOfCrusaders.Powers.Common;

internal class SeethingLifeblood : Power
{
    private bool _takenDamage;

    public override string Name => "Seething Lifeblood";

    public override string Description => "Clearing a room without taking damage sometimes grants lifeblood.";

    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    protected override void Enable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        On.HealthManager.TakeDamage += HealthManager_TakeDamage;
    }

    protected override void Disable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        On.HealthManager.TakeDamage -= HealthManager_TakeDamage;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        if (!_takenDamage && UnityEngine.Random.Range(1, 41) <= CombatController.EnduranceLevel)
            EventRegister.SendEvent("ADD BLUE HEALTH");
        _takenDamage = false;
    }

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        int currentHealth = PDHelper.Health;
        orig(self, hitInstance);
        if (currentHealth != PDHelper.Health)
            _takenDamage = true;
    }
}