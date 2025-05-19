using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class DeepFocus : Power
{
    public override string Name => "Deep Focus";

    public override string Description => "Focus is slower, but restores two health.";

    public override (float, float, float) BonusRates => new(0f, 10f, 30f);

    public override Rarity Tier => Rarity.Uncommon;

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.DeepFocus);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.DeepFocus);
}
