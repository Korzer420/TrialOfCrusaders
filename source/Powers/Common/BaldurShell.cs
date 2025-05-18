using KorzUtils.Enums;
using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class BaldurShell : Power
{
    public override string Name => "Baldur Shell";

    public override string Description => "Grants a shield while focusing.";

    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    internal override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.BaldurShell);

    internal override void Disable() => CharmHelper.UnequipCharm(CharmRef.BaldurShell);
}
