using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Common;

internal class Cocoon : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    public override bool CanAppear => !CombatController.HasPower<InUtterDarkness>(out _);

    public override DraftPool Pools => (DraftPool)((int)DraftPool.Retain * 2 - 1); // All pools

    protected override void Enable()
    {
        for (int i = 0; i < 6; i++)
            EventRegister.SendEvent("ADD BLUE HEALTH");
    }
}
