using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Common;

public class SharedFood : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    public override bool CanAppear => !HasPower<InUtterDarkness>();

    public override DraftPool Pools => DraftPool.Burst | DraftPool.Endurance;

    protected override void Enable() => UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

    protected override void Disable() => UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        if (arg1.name == "GG_Engine_Root")
            HeroController.instance.AddHealth(5);
    }
}
