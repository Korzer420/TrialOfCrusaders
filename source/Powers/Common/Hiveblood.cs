using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Common;

internal class Hiveblood : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    public override bool CanAppear => !HasPower<InUtterDarkness>() && !HasPower<ShiningBound>();

    public override DraftPool Pools => DraftPool.Endurance | DraftPool.Charm;

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.Hiveblood);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.Hiveblood);
}
