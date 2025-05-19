using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class FuryOfTheFallen : Power
{
    public override string Name => "Fury of the Fallen";

    public override string Description => "While at 1 health, increase nail damage significantly.";

    public override (float, float, float) BonusRates => new(40f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.FuryOfTheFallen);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.FuryOfTheFallen);
}
