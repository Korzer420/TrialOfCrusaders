using KorzUtils.Enums;
using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class SoulMaster : Power
{
    private bool _nailUsed;

    public override string Name => "Soul Master";

    public override string Description => "Clearing a room without the nail has a chance to grant a spirit orb.";

    public override (float, float, float) BonusRates => new(0f, 10f, 0f);

    protected override void Enable()
    {
        On.HeroController.Attack += HeroController_Attack;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void HeroController_Attack(On.HeroController.orig_Attack orig, HeroController self, GlobalEnums.AttackDirection attackDir)
    {
        _nailUsed = true;
        orig(self, attackDir);
    }

    protected override void Disable()
    {
        On.HeroController.Attack -= HeroController_Attack;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        _nailUsed = false;
    }
}
