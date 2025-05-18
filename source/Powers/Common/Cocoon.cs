namespace TrialOfCrusaders.Powers.Common;

internal class Cocoon : Power
{
    public override string Name => "Cocoon";

    public override string Description => "Grants 6 lifeblood.";

    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    internal override void Enable()
    {
        for (int i = 0; i < 6; i++)
            EventRegister.SendEvent("ADD BLUE HEALTH");
    }

    internal override void Disable() { }
}
