using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ShapeOfUnn : Power
{
    public override string Name => "Shape of Unn";

    public override string Description => "Focus can be casted while moving on the ground.";

    public override (float, float, float) BonusRates => new(0f, 5f, 35f);

    public override Rarity Tier => Rarity.Uncommon;

    internal override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.ShapeOfUnn);

    internal override void Disable() => CharmHelper.UnequipCharm(CharmRef.ShapeOfUnn);
}
