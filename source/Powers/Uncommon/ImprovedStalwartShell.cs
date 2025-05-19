using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ImprovedStalwartShell : Power
{
    public override string Name => "Improved Stalwart Shell";

    public override string Description => "Extends the invincibility after getting hit.";

    public override (float, float, float) BonusRates => new(0f, 0f, 40f);

    public override Rarity Tier => Rarity.Uncommon;

    protected override void Enable() => HeroController.instance.INVUL_TIME_STAL = 3f;

    protected override void Disable() => HeroController.instance.INVUL_TIME_STAL = 1.75f;
}
