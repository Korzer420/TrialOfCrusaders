using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class FragileStrength : Power
{
    public override (float, float, float) BonusRates => new(40f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public bool StrengthActive { get; set; } = true;

    public override bool CanAppear => !HasPower<PaleShell>();

    protected override void Enable() => CombatController.TookDamage += CombatController_TookDamage;

    protected override void Disable() => CombatController.TookDamage -= CombatController_TookDamage;

    private void CombatController_TookDamage() => StrengthActive = false;

}