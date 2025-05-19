using KorzUtils.Enums;
using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class StalwartShell : Power
{
    public override string Name => "Stalwart Shell";

    public override string Description => "Increases invicibilty after being hit.";

    public override (float, float, float) BonusRates => new(0,0, 10f);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.StalwartShell);

    protected override void Disable() => CharmHelper.LockCharm(CharmRef.StalwartShell);
}
