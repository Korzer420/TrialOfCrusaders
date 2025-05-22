using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Common;

internal class SharpShadow : Power
{
    public override bool CanAppear => PDHelper.HasShadowDash && !CombatController.HasPower<ShiningBound>(out _);

    public override (float, float, float) BonusRates => new(5f, 0f, 5f);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.SharpShadow);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.SharpShadow);
}
