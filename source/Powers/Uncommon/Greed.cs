using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class Greed : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public override bool CanAppear => !HasPower<ShiningBound>() && HasPower<Interest>();

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.FragileGreed);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.FragileGreed);
}
