using KorzUtils.Helper;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class VengefulSpirit : Power
{
    public override (float, float, float) BonusRates => new(0f, 40f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    protected override void Enable()
    {
        if (!HasPower<ShadeSoul>())
            PDHelper.FireballLevel = 1;
        PDHelper.HasSpell = true;
    }

    protected override void Disable()
    {
        PDHelper.FireballLevel = 0;
        PDHelper.HasSpell = false;
    }
}
