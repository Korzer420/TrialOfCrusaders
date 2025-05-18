using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class Grimmchild : Power
{
    public override string Name => "Grubsong";

    public override string Description => "Taking a hit restores soul.";

    public override (float, float, float) BonusRates => new(0f, 20f, 20f);

    public override Rarity Tier => Rarity.Uncommon;

    internal override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.Grimmchild3);

    internal override void Disable() => CharmHelper.UnequipCharm(CharmRef.Grimmchild3);
}
