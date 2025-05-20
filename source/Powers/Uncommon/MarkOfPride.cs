using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class MarkOfPride : Power
{
    public override (float, float, float) BonusRates => new(30f, 0f, 10f);

    public override Rarity Tier => Rarity.Uncommon;

    public override bool CanAppear => HasPower<Longnail>() && !HasPower<ShiningBound>();

    protected override void Enable()
    {
        CharmHelper.EnsureEquipCharm(CharmRef.MarkOfPride);
        CharmHelper.UnequipCharm(CharmRef.Longnail);
    }

    protected override void Disable()
    {
        CharmHelper.UnequipCharm(CharmRef.MarkOfPride);
        // TODO: Check for Longnail
    }
}
