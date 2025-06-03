using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Rare;

internal class TreasureHunter : Power
{
    public override (float, float, float) BonusRates => new(0, 0, 0);

    public override Rarity Tier => Rarity.Rare;
}
