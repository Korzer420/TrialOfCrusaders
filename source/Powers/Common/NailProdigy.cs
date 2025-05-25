using KorzUtils.Helper;
using TrialOfCrusaders.Controller;

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
        CombatController.EnemiesCleared += CombatController_EnemiesCleared;
    }

    private void CombatController_EnemiesCleared()
    {
        if (!_spellUsed)
        {
            if (CombatController.CombatLevel < 20 && !StageController.CurrentRoom.BossRoom && RngProvider.GetStageRandom(1, 20) == 1)
                TreasureManager.SpawnShiny(Enums.TreasureType.CombatOrb, HeroController.instance.transform.position);
            _spellUsed = true;
        }
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
        CombatController.EnemiesCleared += CombatController_EnemiesCleared;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1) => _spellUsed = false;
}