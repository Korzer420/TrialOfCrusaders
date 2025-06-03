using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Rare;

internal class VoidHeart : Power
{
    public override string Name => "Void Heart";

    public override string Description => "???";

    public override (float, float, float) BonusRates => new(33f, 33f, 34f);

    public override Rarity Tier => Rarity.Rare;

    public override bool CanAppear => false;
}
