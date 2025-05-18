using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class InUtterDarkness : Power
{
    public override string Name => "In Utter Darkness";

    public override string Description => $"+4 Survival. You can no longer gain health. Focus now spawns a sibling that attacks enemies.";

    public override (float, float, float) BonusRates => new(40f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    internal override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.Dashmaster);

    internal override void Disable() => CharmHelper.UnequipCharm(CharmRef.Dashmaster);
}
