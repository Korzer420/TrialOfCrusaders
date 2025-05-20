using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Common;

internal class ThornsOfAgony : Power
{
    public override bool CanAppear => !CombatController.HasPower<ShiningBound>(out _);

    public override (float, float, float) BonusRates => new(2f, 0f, 8f);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.ThornsOfAgony);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.ThornsOfAgony);
}
