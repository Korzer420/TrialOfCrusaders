using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Common;

internal class HeavyBlow : Power
{
    public override bool CanAppear => !PowerRef.HasPower<ShiningBound>(out _);

    public override (float, float, float) BonusRates => new(7f, 0f, 3f);

    public override DraftPool Pools => DraftPool.Combat | DraftPool.Endurance | DraftPool.Charm;

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.HeavyBlow);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.HeavyBlow);
}
