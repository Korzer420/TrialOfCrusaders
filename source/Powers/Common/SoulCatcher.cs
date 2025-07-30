using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;
using TrialOfCrusaders.Powers.Uncommon;

namespace TrialOfCrusaders.Powers.Common;

internal class SoulCatcher : Power
{
    public override bool CanAppear => !PowerRef.HasPower<ShiningBound>(out _);

    public override (float, float, float) BonusRates => new(0f, 10f, 0f);

    public override DraftPool Pools => DraftPool.Spirit | DraftPool.Charm;

    protected override void Enable()
    {
        if (!PowerRef.HasPower<SoulEater>(out _))
            CharmHelper.EnsureEquipCharm(CharmRef.SoulCatcher);
    }

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.SoulCatcher);
}
