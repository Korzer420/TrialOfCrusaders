using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Rare;

internal class NailMastersGlory : Power
{
    public override string Name => "Nail Master's Glory";

    public override string Description => "Decreases the charge time of nail arts.";

    public override (float, float, float) BonusRates => new(100f, 0f, 0f);

    public override Rarity Tier => Rarity.Rare;

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.NailmastersGlory);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.NailmastersGlory);
}
