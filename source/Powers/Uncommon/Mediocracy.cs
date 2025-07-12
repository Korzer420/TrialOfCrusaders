using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class Mediocracy : Power
{
    private static int _uncommonRow = 0;

    public override Rarity Tier => Rarity.Uncommon;

    public override DraftPool Pools => DraftPool.Endurance | DraftPool.Treasure;

    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override bool CanAppear => !CombatRef.EnduranceCapped;

    protected override void Enable()
    {
        _uncommonRow = 0;
        TreasureManager.PowerSelected += TreasureManager_PowerSelected;
    }

    protected override void Disable()
    {
        TreasureManager.PowerSelected -= TreasureManager_PowerSelected;
    }

    private void TreasureManager_PowerSelected(Power selectedPower)
    {
        if (selectedPower != null && selectedPower.Tier == Rarity.Uncommon)
        {
            _uncommonRow++;
            if (!CombatRef.EnduranceCapped && RngManager.GetRandom(1, 100) <= _uncommonRow * 2)
                TreasureManager.SpawnShiny(TreasureType.EnduranceOrb, HeroController.instance.transform.position);
        }
        else
            _uncommonRow = 0;
    }
}
