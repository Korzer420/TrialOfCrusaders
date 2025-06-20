using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Common;

internal class Shatter : Power
{
    internal int Stacks { get; set; }

    internal HealthManager LastEnemy { get; set; }

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    public override DraftPool Pools => DraftPool.Combat;

    protected override void Enable() => UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

    protected override void Disable() => UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1) => LastEnemy = null;
}