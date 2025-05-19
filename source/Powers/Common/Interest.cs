namespace TrialOfCrusaders.Powers.Common;

internal class Interest : Power
{
    public override string Name => "Interest";

    public override string Description => "Geo value is slightly increased.";

    public override (float, float, float) BonusRates => new(0f, 0f, 0f);
}
