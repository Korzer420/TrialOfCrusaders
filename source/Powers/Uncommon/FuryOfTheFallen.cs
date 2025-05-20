using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class FuryOfTheFallen : Power
{
    public override (float, float, float) BonusRates => new(40f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public override bool CanAppear => !HasPower<ShiningBound>();

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.FuryOfTheFallen);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.FuryOfTheFallen);
}
