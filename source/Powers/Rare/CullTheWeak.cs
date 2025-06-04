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

    public override bool CanAppear => CombatController.HasPower<ImprovedDefendersCrest>(out _) || CombatController.HasPower<Pyroblast>(out _)
        || CombatController.HasPower<DeepCuts>(out _) || CombatController.HasPower<ScorchingGround>(out _) || CombatController.HasPower<BindingCircle>(out _)
        || CombatController.HasPower<InUtterDarkness>(out _) || CombatController.HasPower<FragileSpirit>(out _);

    protected override void Enable() => CombatController.DebuffsStronger = true;

    protected override void Disable() => CombatController.DebuffsStronger = false;
}