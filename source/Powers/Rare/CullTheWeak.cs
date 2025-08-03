using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.Powers.Uncommon;

namespace TrialOfCrusaders.Powers.Rare;

public class CullTheWeak : Power
{
    public override (float, float, float) BonusRates => new(70f, 30f, 0f);

    public override Rarity Tier => Rarity.Rare;

    public override DraftPool Pools => DraftPool.Debuff;

    public override bool CanAppear => PowerRef.HasPower<ImprovedDefendersCrest>(out _) || PowerRef.HasPower<Pyroblast>(out _)
        || PowerRef.HasPower<DeepCuts>(out _) || PowerRef.HasPower<ScorchingGround>(out _) || PowerRef.HasPower<BindingCircle>(out _)
        || PowerRef.HasPower<InUtterDarkness>(out _) || PowerRef.HasPower<FragileSpirit>(out _);

    protected override void Enable() => PowerRef.DebuffsStronger = true;

    protected override void Disable() => PowerRef.DebuffsStronger = false;
}