using KorzUtils.Enums;
using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class ThornsOfAgony : Power
{
    public override string Name => "Thorns of Agony";

    public override string Description => "Getting hit let thorns attack nearby enemies.";

    public override (float, float, float) BonusRates => new(2f, 0f, 8f);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.ThornsOfAgony);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.ThornsOfAgony);
}
