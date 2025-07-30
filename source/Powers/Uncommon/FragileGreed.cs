using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class FragileGreed : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public override DraftPool Pools => DraftPool.Wealth;

    public bool GreedActive { get; set; } = true;

    public override bool CanAppear => !HasPower<PaleShell>();

    protected override void Enable() => CombatRef.TookDamage += CombatController_TookDamage;

    protected override void Disable()
    { 
        CombatRef.TookDamage -= CombatController_TookDamage;
        GreedActive = true;
    }

    private void CombatController_TookDamage() => GreedActive = false;
}