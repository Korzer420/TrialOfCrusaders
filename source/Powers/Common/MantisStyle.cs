using TrialOfCrusaders.Data;

namespace TrialOfCrusaders.Powers.Common;

internal class MantisStyle : Power
{
    internal bool Parried { get; set; }

    public override (float, float, float) BonusRates => new(9f, 0f, 1f);

    protected override void Enable()
    {
        On.HeroController.NailParry += HeroController_NailParry;
        On.HeroController.CycloneInvuln += HeroController_CycloneInvuln;
    }

    protected override void Disable()
    {
        On.HeroController.NailParry -= HeroController_NailParry;
        On.HeroController.CycloneInvuln -= HeroController_CycloneInvuln;
        Parried = false;
    }

    private void HeroController_CycloneInvuln(On.HeroController.orig_CycloneInvuln orig, HeroController self)
    {
        orig(self);
        Parried = true;
    }

    private void HeroController_NailParry(On.HeroController.orig_NailParry orig, HeroController self)
    {
        orig(self);
        Parried = true;
    }
}