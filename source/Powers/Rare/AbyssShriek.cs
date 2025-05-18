using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Rare;

internal class AbyssShriek : Power
{
    public override string Name => "Abyss Shriek";

    public override string Description => "Unlocks Abyss Shriek";

    public override (float, float, float) BonusRates => new(0f, 100f, 0f);

    public override Rarity Tier => Rarity.Rare;

    internal override void Enable()
    {
        PDHelper.ScreamLevel = 2;
        PDHelper.HasSpell = true;
    }

    internal override void Disable()
    {
        PDHelper.ScreamLevel = 0;
        PDHelper.HasSpell = false;
    }
}
