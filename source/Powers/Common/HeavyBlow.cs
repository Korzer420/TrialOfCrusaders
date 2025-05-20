using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Common;

internal class HeavyBlow : Power
{
    public override bool CanAppear => !CombatController.HasPower<ShiningBound>(out _);

    public override (float, float, float) BonusRates => new(7f, 0f, 3f);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.HeavyBlow);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.HeavyBlow);
}
