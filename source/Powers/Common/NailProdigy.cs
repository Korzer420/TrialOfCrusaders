using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class NailProdigy : Power
{
    private bool _spellUsed;

    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override bool CanAppear => CombatController.CombatLevel < 20;

    protected override void Enable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        On.HutongGames.PlayMaker.Actions.SetVelocity2d.OnEnter += SetVelocity2d_OnEnter;
        On.HutongGames.PlayMaker.Actions.Tk2dPlayFrame.OnEnter += Tk2dPlayFrame_OnEnter;
        StageController.RoomCleared += StageController_RoomCleared;
    }

    private void StageController_RoomCleared()
    {
        if (!_spellUsed && CombatController.CombatLevel < 20 && RngProvider.GetStageRandom(1, 10) == 1)
            TreasureController.SpawnShiny(Enums.TreasureType.CombatOrb, HeroController.instance.transform.position);
    }

    private void Tk2dPlayFrame_OnEnter(On.HutongGames.PlayMaker.Actions.Tk2dPlayFrame.orig_OnEnter orig, HutongGames.PlayMaker.Actions.Tk2dPlayFrame self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Focus Heal*"))
            _spellUsed = true;
        orig(self);
    }

    private void SetVelocity2d_OnEnter(On.HutongGames.PlayMaker.Actions.SetVelocity2d.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetVelocity2d self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Spell End"))
            _spellUsed = true;
        orig(self);
    }

    protected override void Disable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        On.HutongGames.PlayMaker.Actions.SetVelocity2d.OnEnter -= SetVelocity2d_OnEnter;
        On.HutongGames.PlayMaker.Actions.Tk2dPlayFrame.OnEnter -= Tk2dPlayFrame_OnEnter;
        StageController.RoomCleared -= StageController_RoomCleared;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        _spellUsed = false;
    }
}