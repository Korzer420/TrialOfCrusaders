using MonoMod.Cil;
using System;

namespace TrialOfCrusaders.Powers.Common;

internal class EscapeArtist : Power
{
    public override string Name => "Escape Artist";

    public override string Description => "Increases your movement speed while recovering from a hit.";

    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    protected override void Enable() => IL.HeroController.Move += HeroController_Move;
    
    protected override void Disable() => IL.HeroController.Move -= HeroController_Move;

    private void HeroController_Move(ILContext il)
    {
        
    }
}