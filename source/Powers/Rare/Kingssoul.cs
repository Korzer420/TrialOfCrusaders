using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Rare;

internal class Kingssoul : Power
{
    public override (float, float, float) BonusRates => new(0f, 100f, 0f);

    public override Rarity Tier => Rarity.Rare;

    public override bool CanAppear => !HasPower<ShiningBound>();

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.Kingssoul);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.Kingssoul);
}
