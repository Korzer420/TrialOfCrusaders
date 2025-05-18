using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Rare;

internal class DreamWielder : Power
{
    public override string Name => "Dream Wielder";

    public override string Description => "Increases dream nail power and speed.";

    public override (float, float, float) BonusRates => new(20f, 60f, 20f);

    public override Rarity Tier => Rarity.Rare;

    internal override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.DreamWielder);

    internal override void Disable() => CharmHelper.UnequipCharm(CharmRef.DreamWielder);
}
