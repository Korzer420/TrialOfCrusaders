using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Rare;

internal class Gambling : Power
{
    public int LeftRolls { get; set; }

    public override Rarity Tier => Rarity.Rare;

    public override DraftPool Pools => DraftPool.Treasure | DraftPool.Wealth;

    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    protected override void Enable() => LeftRolls = 0;
}
