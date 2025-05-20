namespace TrialOfCrusaders.Powers.Common;

internal class SpellProdigy : Power
{
    private bool _nailUsed;

    public override bool CanAppear => CombatController.HasSpell();

    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    protected override void Enable()
    {
        On.HeroController.Attack += HeroController_Attack;
        StageController.RoomCleared += StageController_RoomCleared;
    }

    protected override void Disable()
    {
        On.HeroController.Attack -= HeroController_Attack;
        StageController.RoomCleared -= StageController_RoomCleared;
    }

    private void HeroController_Attack(On.HeroController.orig_Attack orig, HeroController self, GlobalEnums.AttackDirection attackDir)
    {
        _nailUsed = true;
        orig(self, attackDir);
    }

    private void StageController_RoomCleared()
    {
        if (!_nailUsed && CombatController.SpiritLevel < 20 && RngProvider.GetStageRandom(1, 10) <= 2)
            TreasureController.SpawnShiny(Enums.TreasureType.SpiritOrb, HeroController.instance.transform.position);
        _nailUsed = false;
    }
}
