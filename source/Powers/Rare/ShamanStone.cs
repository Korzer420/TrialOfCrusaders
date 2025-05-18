using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Rare;

internal class ShamanStone : Power
{
    public override string Name => "Shaman Stone";

    public override string Description => "Empowers all spells.";

    public override (float, float, float) BonusRates => new(0f, 100f, 0f);

    public override Rarity Tier => Rarity.Rare;

    internal override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.ShamanStone);

    internal override void Disable() => CharmHelper.UnequipCharm(CharmRef.ShamanStone);
}
