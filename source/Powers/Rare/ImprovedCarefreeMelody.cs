using MonoMod.Cil;
using System;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Rare;

internal class ImprovedCarefreeMelody : Power
{
    public override string Name => "Improved Carefree Melody";

    public override string Description => "Avoids damage more often.";

    public override (float, float, float) BonusRates => new(0f, 0f, 100f);

    public override Rarity Tier => Rarity.Rare;

    internal override void Enable() => IL.HeroController.TakeDamage += HeroController_TakeDamage;
    
    internal override void Disable() => IL.HeroController.TakeDamage -= HeroController_TakeDamage;

    private void HeroController_TakeDamage(MonoMod.Cil.ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.Goto(0);
        cursor.GotoNext(MoveType.Before, x => x.MatchLdarg(0), x => x.MatchLdcI4(0), x => x.MatchStfld<HeroController>("hitsSinceShielded"));
        cursor.GotoNext(MoveType.After, x => x.MatchLdcI4(0));
        cursor.EmitDelegate<Func<int, int>>(x => 2);
    }
}
