using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Common;

internal class GatheringSwarm : Power
{
    public override bool CanAppear => !CombatController.HasPower<ShiningBound>(out _);

    public override (float, float, float) BonusRates => new(0f, 0, 0f);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.GatheringSwarm);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.GatheringSwarm);
}
