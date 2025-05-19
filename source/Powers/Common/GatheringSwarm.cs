using KorzUtils.Enums;
using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class GatheringSwarm : Power
{
    public override string Name => "Gathering Swarm";

    public override string Description => "Pulls nearby geo at you.";

    public override (float, float, float) BonusRates => new(0f, 0, 0f);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.GatheringSwarm);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.GatheringSwarm);
}
