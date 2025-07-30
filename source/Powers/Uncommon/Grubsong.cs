using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class Grubsong : Power
{
    public override (float, float, float) BonusRates => new(0f, 20f, 20f);

    public override Rarity Tier => Rarity.Uncommon;

    public override bool CanAppear => !HasPower<ShiningBound>();

    public override DraftPool Pools => DraftPool.Charm;

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.Grubsong);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.Grubsong);
}