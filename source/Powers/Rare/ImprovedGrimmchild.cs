using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Rare;

internal class ImprovedGrimmchild : Power
{
    public override (float, float, float) BonusRates => new(95f, 0f, 5f);

    public override Rarity Tier => Rarity.Rare;

    public override bool CanAppear => HasPower<ImprovedGrimmchild>();

    public override Sprite Sprite => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + GetType().Name);

    protected override void Enable()
    {
        CharmHelper.EnsureEquipCharm(KorzUtils.Enums.CharmRef.Grimmchild4);
        On.GetHP.OnEnter += GetHP_OnEnter;
    }

    protected override void Disable()
    {
        CharmHelper.UnequipCharm(KorzUtils.Enums.CharmRef.Grimmchild4);
        On.GetHP.OnEnter -= GetHP_OnEnter;
    }

    private void GetHP_OnEnter(On.GetHP.orig_OnEnter orig, GetHP self)
    {
        int normalDamage = self.Fsm.Variables.FindFsmInt("Damage").Value;
        if (self.IsCorrectContext("Attack", "Enemy Damager", "Hit") && self.Fsm.GetState("Send Impact") != null)
            self.Fsm.Variables.FindFsmInt("Damage").Value = 15 + (CombatController.CombatLevel * 10);
        orig(self);
        if (self.IsCorrectContext("Attack", "Enemy Damager", "Hit") && self.Fsm.GetState("Send Impact") != null)
            self.Fsm.Variables.FindFsmInt("Damage").Value = normalDamage;
    }
}
