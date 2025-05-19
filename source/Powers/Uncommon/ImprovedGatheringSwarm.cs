using MonoMod.Cil;
using System;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ImprovedGatheringSwarm : Power
{
    public override string Name => "Improved Gathering Swarm";

    public override string Description => "Gathers geo more efficient.";

    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    protected override void Enable() => IL.GeoControl.FixedUpdate += GeoControl_FixedUpdate;

    protected override void Disable() => IL.GeoControl.FixedUpdate -= GeoControl_FixedUpdate;

    private void GeoControl_FixedUpdate(MonoMod.Cil.ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.Goto(0);
        cursor.GotoNext(MoveType.After, x => x.MatchLdcR4(150f));
        cursor.EmitDelegate<Func<float, float>>(x => 450f);
        cursor.GotoNext(MoveType.After, x => x.MatchLdcR4(150f));
        cursor.EmitDelegate<Func<float, float>>(x => 450f);
    }
}
