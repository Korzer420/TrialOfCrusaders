using KorzUtils.Enums;
using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class NailMaster : Power
{
    private bool _spellUsed;

    public override string Name => "Nail Master";

    public override string Description => "Clearing a room without casting a spell/using focus has a small chance to spawn a combat orb.";

    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    internal override void Enable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        On.HutongGames.PlayMaker.Actions.SetVelocity2d.OnEnter += SetVelocity2d_OnEnter;
        On.HutongGames.PlayMaker.Actions.Tk2dPlayFrame.OnEnter += Tk2dPlayFrame_OnEnter;
    }

    private void Tk2dPlayFrame_OnEnter(On.HutongGames.PlayMaker.Actions.Tk2dPlayFrame.orig_OnEnter orig, HutongGames.PlayMaker.Actions.Tk2dPlayFrame self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Focus Heal"))
            _spellUsed = true;
        orig(self);
    }

    private void SetVelocity2d_OnEnter(On.HutongGames.PlayMaker.Actions.SetVelocity2d.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetVelocity2d self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Spell End"))
            _spellUsed = true;
        orig(self);
    }

    internal override void Disable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        On.HutongGames.PlayMaker.Actions.SetVelocity2d.OnEnter -= SetVelocity2d_OnEnter;
        On.HutongGames.PlayMaker.Actions.Tk2dPlayFrame.OnEnter -= Tk2dPlayFrame_OnEnter;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        _spellUsed = false;
    }
}