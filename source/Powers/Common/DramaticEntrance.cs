using KorzUtils.Helper;
using System.Reflection;
using TrialOfCrusaders.Controller;

namespace TrialOfCrusaders.Powers.Common;

internal class DramaticEntrance : Power
{
    private MethodInfo _takeDamage;

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    protected override void Enable()
    {
        _takeDamage = typeof(HealthManager).GetMethod("TakeDamage", BindingFlags.Instance | BindingFlags.NonPublic);
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        HitInstance hitInstance = new()
        {
            AttackType = AttackTypes.Generic,
            Source = HeroController.instance?.gameObject,
            Multiplier = 1,
            IsExtraDamage = true,
            DamageDealt = 10 + CombatController.CombatLevel * 2
        };
        foreach (HealthManager enemy in CombatController.Enemies)
            _takeDamage.Invoke(enemy, [hitInstance]);
    }

    protected override void Disable() => UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
}