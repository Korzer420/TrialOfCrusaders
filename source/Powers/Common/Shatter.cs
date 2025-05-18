namespace TrialOfCrusaders.Powers.Common;

internal class Shatter : Power
{
    private HealthManager _lastEnemy;
    private int _stacks = 0;

    public override string Name => "Shatter";

    public override string Description => "Consecutive hits onto the same target reduce the armor.";

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    internal override void Enable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        On.HealthManager.TakeDamage += HealthManager_TakeDamage;
    }

    internal override void Disable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        On.HealthManager.TakeDamage -= HealthManager_TakeDamage;
    }

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        if (hitInstance.AttackType == AttackTypes.Nail)
        {
            if (_lastEnemy == self)
                _stacks++;
            else
            {
                _stacks = 0;
                _lastEnemy = self;
            }
        }
        orig(self, hitInstance);
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        _lastEnemy = null;
    }
}