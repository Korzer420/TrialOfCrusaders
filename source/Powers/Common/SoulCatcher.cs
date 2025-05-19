using KorzUtils.Enums;
using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class SoulCatcher : Power
{
    public override string Name => "Soul Catcher";

    public override string Description => "Slightly increases the soul gained from enemies.";

    public override (float, float, float) BonusRates => new(0f, 10f, 0f);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.SoulCatcher);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.SoulCatcher);
}
