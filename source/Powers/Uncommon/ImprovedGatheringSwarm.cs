using MonoMod.Cil;
using System;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ImprovedGatheringSwarm : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public override DraftPool Pools => DraftPool.Wealth | DraftPool.Charm | DraftPool.Upgrade;

    public override bool CanAppear => HasPower<GatheringSwarm>();

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
