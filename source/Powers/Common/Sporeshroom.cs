using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Common;

internal class Sporeshroom : Power
{
    public override bool CanAppear => !PowerRef.HasPower<ShiningBound>(out _);

    public override (float, float, float) BonusRates => new(0f, 6f, 4f);

    public override DraftPool Pools => DraftPool.Combat | DraftPool.Charm | DraftPool.Upgrade;

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.Sporeshroom);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.Sporeshroom);
}
