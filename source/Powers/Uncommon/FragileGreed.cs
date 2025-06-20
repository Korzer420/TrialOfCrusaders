using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class FragileGreed : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public override DraftPool Pools => DraftPool.Wealth;

    public bool GreedActive { get; set; } = true;

    public override bool CanAppear => !HasPower<PaleShell>();

    public override Sprite Sprite => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + GetType().Name);

    protected override void Enable() => CombatController.TookDamage += CombatController_TookDamage;

    protected override void Disable()
    { 
        CombatController.TookDamage -= CombatController_TookDamage;
        GreedActive = true;
    }

    private void CombatController_TookDamage() => GreedActive = false;
}