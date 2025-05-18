using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class SpellTwister : Power
{
    public override string Name => "Spell Twister";

    public override string Description => "Decreases the cost of spells.";

    public override (float, float, float) BonusRates => new(0f, 40f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    internal override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.SpellTwister);

    internal override void Disable() => CharmHelper.UnequipCharm(CharmRef.SpellTwister);
}
