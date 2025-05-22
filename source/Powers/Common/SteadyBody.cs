using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Common;

internal class SteadyBody : Power
{
    public override bool CanAppear => !CombatController.HasPower<ShiningBound>(out _);

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.SteadyBody);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.SteadyBody);
}
