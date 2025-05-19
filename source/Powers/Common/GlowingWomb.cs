using KorzUtils.Enums;
using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class GlowingWomb : Power
{
    public override string Name => "Glowing Womb";

    public override string Description => "Periodically spawns a hatchling that attacks enemies. Consumes Soul.";

    public override (float, float, float) BonusRates => new(2f, 8f, 0f);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.GlowingWomb);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.GlowingWomb);
}
