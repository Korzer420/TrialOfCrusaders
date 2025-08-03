using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Rare;

internal class ImprovedGrimmchild : Power
{
    public override (float, float, float) BonusRates => new(95f, 0f, 5f);

    public override Rarity Tier => Rarity.Rare;

    public override StatScaling Scaling => StatScaling.Combat;

    public override DraftPool Pools => DraftPool.Combat | DraftPool.Upgrade | DraftPool.Charm;

    public override bool CanAppear => HasPower<ImprovedGrimmchild>();

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
        bool correctContext = self.IsCorrectContext("Attack", "Enemy Damager", "Hit") && self.Fsm.GetState("Send Impact") != null;
        if (correctContext)
            self.Fsm.Variables.FindFsmInt("Damage").Value = 15 + (CombatRef.CombatLevel * 10);
        orig(self);
        if (correctContext)
            self.Fsm.Variables.FindFsmInt("Damage").Value = normalDamage;
    }
}
