namespace TrialOfCrusaders.Powers.Common;

public class CullTheWeak : Power
{
    public override string Name => "Cull the weak";

    public override string Description => "Status effects are stronger.";

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    protected override void Enable()
    {
        // ToDo after status implementation.
    }

    protected override void Disable()
    {
    }
}