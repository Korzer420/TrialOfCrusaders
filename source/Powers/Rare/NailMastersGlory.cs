using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Rare;

internal class NailmastersGlory : Power
{
    public override (float, float, float) BonusRates => new(100f, 0f, 0f);

    public override Rarity Tier => Rarity.Rare;

    public override DraftPool Pools => DraftPool.Charm | DraftPool.Combat | DraftPool.Upgrade | DraftPool.Ability;

    public override bool CanAppear => !HasPower<ShiningBound>() && PowerRef.HasNailArt();

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.NailmastersGlory);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.NailmastersGlory);
}
