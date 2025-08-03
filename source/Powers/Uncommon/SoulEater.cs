using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class SoulEater : Power
{
    public override Rarity Tier => Rarity.Uncommon;

    public override (float, float, float) BonusRates => new(0f, 40f, 0f);

    public override DraftPool Pools => DraftPool.Spirit | DraftPool.Charm | DraftPool.Upgrade;

    public override bool CanAppear => HasPower<SoulCatcher>() && !HasPower<ShiningBound>();

    protected override void Enable() 
    { 
        CharmHelper.EnsureEquipCharm(CharmRef.SoulEater); 
        CharmHelper.UnequipCharm(CharmRef.SoulCatcher);
    }

    protected override void Disable()
    {
        CharmHelper.UnequipCharm(CharmRef.SoulEater);
    }
}
