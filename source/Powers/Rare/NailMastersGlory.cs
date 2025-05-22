using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Rare;

internal class NailMastersGlory : Power
{
    public override (float, float, float) BonusRates => new(100f, 0f, 0f);

    public override Rarity Tier => Rarity.Rare;

    public override bool CanAppear => !HasPower<ShiningBound>() && CombatController.HasNailArt();

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.NailmastersGlory);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.NailmastersGlory);
}
