using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;
using TrialOfCrusaders.Powers.Uncommon;

namespace TrialOfCrusaders.Powers.Common;

internal class Longnail : Power
{
    public override bool CanAppear => !PowerRef.HasPower<ShiningBound>(out _);

    public override (float, float, float) BonusRates => new(7.5f, 0f, 2.5f);

    public override DraftPool Pools => DraftPool.Combat | DraftPool.Charm;

    protected override void Enable()
    { 
        if (!PowerRef.HasPower<MarkOfPride>(out _))
            CharmHelper.EnsureEquipCharm(CharmRef.Longnail);
    }

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.Longnail);
}
