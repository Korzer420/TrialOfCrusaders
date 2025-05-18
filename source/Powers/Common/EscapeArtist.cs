using MonoMod.Cil;
using System;

namespace TrialOfCrusaders.Powers.Common;

internal class EscapeArtist : Power
{
    public override string Name => "Escape Artist";

    public override string Description => "Increases your movement speed while recovering from a hit.";

    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    internal override void Enable() => IL.HeroController.Move += HeroController_Move;
    
    internal override void Disable() => IL.HeroController.Move -= HeroController_Move;

    private void HeroController_Move(ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.Goto(0);
        cursor.GotoNext(MoveType.After, x => x.MatchLdfld<HeroController>(nameof(HeroController.RUN_SPEED_CH_COMBO)));
        cursor.EmitDelegate<Func<float, float>>(x => x + (HeroController.instance.cState.invulnerable ? 6f : 0f));
        cursor.GotoNext(MoveType.After, x => x.MatchLdfld<HeroController>(nameof(HeroController.RUN_SPEED_CH)));
        cursor.EmitDelegate<Func<float, float>>(x => x + (HeroController.instance.cState.invulnerable ? 6f : 0f));
        cursor.GotoNext(MoveType.After, x => x.MatchLdfld<HeroController>(nameof(HeroController.RUN_SPEED)));
        cursor.EmitDelegate<Func<float, float>>(x => x + (HeroController.instance.cState.invulnerable ? 6f : 0f));
    }
}