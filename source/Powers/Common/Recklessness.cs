namespace TrialOfCrusaders.Powers.Common;

internal class Recklessness : Power
{
    public override string Name => "Recklessness";

    public override string Description => "Great Slash and Dash Slash deal 400% increased damage, but you take damage if this doesn't kill the enemy.";

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);
}
