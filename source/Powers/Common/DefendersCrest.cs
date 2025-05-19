using KorzUtils.Enums;
using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class DefendersCrest : Power
{
    public override string Name => "Defender's Crest";

    public override string Description => "Emits a poison cloud.";

    public override (float, float, float) BonusRates => new(5f, 0f, 5f);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.DefendersCrest);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.DefendersCrest);
}
