using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;

namespace TrialOfCrusaders.Powers.Rare;

internal class DreamWielder : Power
{
    public override (float, float, float) BonusRates => new(20f, 60f, 20f);

    public override Rarity Tier => Rarity.Rare;

    public override DraftPool Pools => DraftPool.Spirit | DraftPool.Upgrade | DraftPool.Charm | DraftPool.Ability;

    public override bool CanAppear => PowerRef.HasPower<DreamNail>(out _) && !PowerRef.HasPower<ShiningBound>(out _);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.DreamWielder);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.DreamWielder);
}
