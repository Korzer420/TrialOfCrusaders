using TrialOfCrusaders.Powers.Uncommon;

namespace TrialOfCrusaders.Powers.Common;

internal class Cocoon : Power
{
    public bool Activated { get; set; }

    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    public override bool CanAppear => !CombatController.HasPower<InUtterDarkness>(out _);

    protected override void Enable()
    {
        if (!Activated)
            for (int i = 0; i < 6; i++)
                EventRegister.SendEvent("ADD BLUE HEALTH");
        Activated = true;
    }
}
