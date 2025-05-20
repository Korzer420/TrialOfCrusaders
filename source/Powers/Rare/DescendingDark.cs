using KorzUtils.Helper;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Uncommon;

namespace TrialOfCrusaders.Powers.Rare;

internal class DescendingDark : Power
{
    public override (float, float, float) BonusRates => new(0f, 100f, 0f);

    public override Rarity Tier => Rarity.Rare;

    public override bool CanAppear => CombatController.HasPower<DesolateDive>(out _);

    protected override void Enable()
    {
        PDHelper.QuakeLevel = 2;
        PDHelper.HasSpell = true;
    }

    protected override void Disable()
    {
        PDHelper.QuakeLevel = 0;
        PDHelper.HasSpell = false;
    }
}
