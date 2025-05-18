using KorzUtils.Enums;
using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class SprintMaster : Power
{
    public override string Name => "Sprintmaster";

    public override string Description => "Increases movement speed on the ground.";

    public override (float, float, float) BonusRates => new(5f, 0f, 5f);

    internal override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.Sprintmaster);

    internal override void Disable() => CharmHelper.UnequipCharm(CharmRef.Sprintmaster);
}
