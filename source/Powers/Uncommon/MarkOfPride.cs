using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class MarkOfPride : Power
{
    public override string Name => "Mark of Pride";

    public override string Description => "Increases the nail range significantly. Overwrites Longnail.";

    public override (float, float, float) BonusRates => new(30f, 0f, 10f);

    public override Rarity Tier => Rarity.Uncommon;

    internal override void Enable()
    {
        CharmHelper.EnsureEquipCharm(CharmRef.MarkOfPride);
        CharmHelper.UnequipCharm(CharmRef.Longnail);
    }

    internal override void Disable()
    {
        CharmHelper.UnequipCharm(CharmRef.MarkOfPride);
        // TODO: Check for Longnail
    }
}
