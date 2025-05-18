using KorzUtils.Helper;
using System.Reflection;

namespace TrialOfCrusaders.Powers.Common;

internal class DramaticEntrance : Power
{
    private MethodInfo _takeDamage;

    public override string Name => "Dramatic Entrance";

    public override string Description => "Entering a room deals a bit of damage to all enemies.";

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    internal override void Enable()
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

    internal override void Disable() => UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
}