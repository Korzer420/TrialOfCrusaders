using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class Dashmaster : Power
{
    public override string Name => "Dashmaster";

    public override string Description => "Decreases the dash cooldown. Allows dashing downwards.";

    public override (float, float, float) BonusRates => new(0f, 0f, 40f);

    public override Rarity Tier => Rarity.Uncommon;

    internal override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.Dashmaster);

    internal override void Disable() => CharmHelper.UnequipCharm(CharmRef.Dashmaster);
}
