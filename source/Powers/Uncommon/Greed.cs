using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class Greed : Power
{
    public override string Name => "Greed";

    public override string Description => "Doubles the geo dropped from enemies.";

    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    internal override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.FragileGreed);

    internal override void Disable() => CharmHelper.UnequipCharm(CharmRef.FragileGreed);
}
