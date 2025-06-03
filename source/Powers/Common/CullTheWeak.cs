using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Powers.Uncommon;

namespace TrialOfCrusaders.Powers.Common;

public class CullTheWeak : Power
{
    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    public override bool CanAppear => CombatController.HasPower<ImprovedDefendersCrest>(out _ ) || CombatController.HasPower<Pyroblast>(out _) 
        || CombatController.HasPower<DeepCuts>(out _) || CombatController.HasPower<ScorchingGround>(out _) || CombatController.HasPower<BindingCircle>(out _);
}