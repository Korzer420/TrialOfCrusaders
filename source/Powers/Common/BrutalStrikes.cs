namespace TrialOfCrusaders.Powers.Common;

internal class BrutalStrikes : Power
{
    public override string Name => "Brutal strikes";

    public override string Description => "Nail arts ignore armor.";

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    internal override void Enable()
    {
        // ToDo after implementing armor.
    }

    internal override void Disable()
    {

    }
}