using HutongGames.PlayMaker.Actions;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class FragileGreed : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public bool GreedActive { get; set; } = true;

    public override bool CanAppear => !HasPower<PaleShell>();

    protected override void Enable() => CombatController.TookDamage += CombatController_TookDamage;

    protected override void Disable() => CombatController.TookDamage -= CombatController_TookDamage;

    private void CombatController_TookDamage() => GreedActive = false;
}