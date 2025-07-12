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

    public override bool CanAppear => CombatRef.HasPower<ImprovedDefendersCrest>(out _) || CombatRef.HasPower<Pyroblast>(out _)
        || CombatRef.HasPower<DeepCuts>(out _) || CombatRef.HasPower<ScorchingGround>(out _) || CombatRef.HasPower<BindingCircle>(out _)
        || CombatRef.HasPower<InUtterDarkness>(out _) || CombatRef.HasPower<FragileSpirit>(out _);

    protected override void Enable() => CombatRef.DebuffsStronger = true;

    protected override void Disable() => CombatRef.DebuffsStronger = false;
}