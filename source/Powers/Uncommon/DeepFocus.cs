using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class DeepFocus : Power
{
    public override (float, float, float) BonusRates => new(0f, 10f, 30f);

    public override Rarity Tier => Rarity.Uncommon;

    public override bool CanAppear => !HasPower<ShiningBound>();

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.DeepFocus);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.DeepFocus);
}
