using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class SpellTwister : Power
{
    public override (float, float, float) BonusRates => new(0f, 40f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public override bool CanAppear => !HasPower<ShiningBound>();

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.SpellTwister);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.SpellTwister);
}
