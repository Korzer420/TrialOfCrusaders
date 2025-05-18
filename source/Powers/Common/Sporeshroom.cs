using KorzUtils.Enums;
using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class Sporeshroom : Power
{
    public override string Name => "Sporeshroom";

    public override string Description => "Focus emits a spore cloud.";

    public override (float, float, float) BonusRates => new(0f, 6f, 4f);

    internal override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.Sporeshroom);

    internal override void Disable() => CharmHelper.UnequipCharm(CharmRef.Sporeshroom);
}
