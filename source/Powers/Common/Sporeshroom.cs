using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Common;

internal class Sporeshroom : Power
{
    public override bool CanAppear => !CombatController.HasPower<ShiningBound>(out _);

    public override (float, float, float) BonusRates => new(0f, 6f, 4f);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.Sporeshroom);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.Sporeshroom);
}
