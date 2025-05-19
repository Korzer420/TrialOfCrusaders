using KorzUtils.Enums;
using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class HeavyBlow : Power
{
    public override string Name => "Heavy Blow";

    public override string Description => "Increases knockback from hitting enemies.";

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.HeavyBlow);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.HeavyBlow);
}
