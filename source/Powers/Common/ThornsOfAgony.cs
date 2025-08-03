using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Common;

internal class ThornsOfAgony : Power
{
    public override bool CanAppear => !PowerRef.HasPower<ShiningBound>(out _);

    public override (float, float, float) BonusRates => new(2f, 0f, 8f);

    public override DraftPool Pools => DraftPool.Combat | DraftPool.Charm;

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.ThornsOfAgony);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.ThornsOfAgony);
}
