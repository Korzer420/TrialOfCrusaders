using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class QuickFocus : Power
{
    public override string Name => "Quick Focus";

    public override string Description => "Increase focus speed.";

    public override (float, float, float) BonusRates => new(0f, 10f, 30f);

    public override Rarity Tier => Rarity.Uncommon;

    internal override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.QuickFocus);

    internal override void Disable() => CharmHelper.UnequipCharm(CharmRef.QuickFocus);
}
