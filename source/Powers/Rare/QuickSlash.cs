using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Rare;

internal class QuickSlash : Power
{
    public override string Name => "Quick Slash";

    public override string Description => "Increases attack speed.";

    public override (float, float, float) BonusRates => new(100f, 0f, 0f);

    public override Rarity Tier => Rarity.Rare;

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.QuickSlash);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.QuickSlash);
}
