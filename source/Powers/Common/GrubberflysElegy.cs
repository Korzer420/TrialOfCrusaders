using KorzUtils.Enums;
using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class GrubberflysElegy : Power
{
    public override string Name => "Grubberfly's Elegy";

    public override string Description => "While at full health fire waves with your nail.";

    public override (float, float, float) BonusRates => new(5f, 0f, 5f);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.GrubberflyElegy);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.GrubberflyElegy);
}
