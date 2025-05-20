using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Common;

internal class SprintMaster : Power
{
    public override bool CanAppear => !CombatController.HasPower<ShiningBound>(out _);

    public override (float, float, float) BonusRates => new(5f, 0f, 5f);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.Sprintmaster);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.Sprintmaster);
}
