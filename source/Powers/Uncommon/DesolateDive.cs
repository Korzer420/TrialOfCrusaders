using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class DesolateDive : Power
{
    public override (float, float, float) BonusRates => new(0f, 40f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public override DraftPool Pools => DraftPool.Spirit | DraftPool.Ability;

    protected override void Enable()
    {
        if (!HasPower<DescendingDark>())
            PDHelper.QuakeLevel = 1;
        PDHelper.HasSpell = true;
    }

    protected override void Disable()
    {
        PDHelper.QuakeLevel = 0;
        PDHelper.HasSpell = false;
    }
}
