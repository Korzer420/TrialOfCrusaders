using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;

namespace TrialOfCrusaders.Powers.Common;

internal class SpellProdigy : Power
{
    private bool _nailUsed;

    public override bool CanAppear => CombatController.HasSpell() && !CombatController.SpiritCapped;

    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override DraftPool Pools => DraftPool.Treasure;

    protected override void Enable()
    {
        On.HeroController.Attack += HeroController_Attack;
        CombatController.EnemiesCleared += CombatController_EnemiesCleared;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    protected override void Disable()
    {
        On.HeroController.Attack -= HeroController_Attack;
        CombatController.EnemiesCleared -= CombatController_EnemiesCleared;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
    }

    private void HeroController_Attack(On.HeroController.orig_Attack orig, HeroController self, GlobalEnums.AttackDirection attackDir)
    {
        _nailUsed = true;
        orig(self, attackDir);
    }

    private void CombatController_EnemiesCleared()
    {
        if (!_nailUsed && !CombatController.SpiritCapped && RngManager.GetRandom(1, 10) <= 2)
            TreasureManager.SpawnShiny(Enums.TreasureType.SpiritOrb, HeroController.instance.transform.position);
        _nailUsed = true;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1) => _nailUsed = false;
}
