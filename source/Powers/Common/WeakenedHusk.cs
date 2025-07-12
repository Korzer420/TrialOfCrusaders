using KorzUtils.Helper;
using Modding.Utils;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;
using TrialOfCrusaders.UnityComponents.Debuffs;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class WeakenedHusk : Power
{
    public override (float, float, float) BonusRates => new(0f, 10f, 0f);

    public override bool CanAppear => CombatRef.HasSpell();

    public override StatScaling Scaling => StatScaling.Spirit;

    public override DraftPool Pools => DraftPool.Spirit | DraftPool.Debuff | DraftPool.Upgrade;

    protected override void Enable() => On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter += TakeDamage_OnEnter;

    protected override void Disable() => On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter -= TakeDamage_OnEnter;

    private void TakeDamage_OnEnter(On.HutongGames.PlayMaker.Actions.TakeDamage.orig_OnEnter orig, HutongGames.PlayMaker.Actions.TakeDamage self)
    {
        if (self.IsCorrectContext("damages_enemy", null, "Send Event"))
        {
            string gameObjectName = self.Fsm.GameObjectName;
            string parentName = self.Fsm.GameObject.transform.parent?.name;
            if (gameObjectName.Contains("Fireball")
                || gameObjectName == "Q Fall Damage"
                || (gameObjectName == "Hit U" && (parentName == "Scr Heads" || parentName == "Scr Heads 2"))
                || ((gameObjectName == "Hit R" || gameObjectName == "Hit L") && (parentName == "Q Slam" || parentName == "Q Slam 2" || parentName == "Q Mega" || parentName == "Scr Heads" || parentName == "Scr Heads 2")))
            {
                if (RngManager.GetRandom(0, 100) <= Mathf.CeilToInt(CombatRef.SpiritLevel * 1.5f))
                    self.Fsm.GameObject.GetOrAddComponent<ShatteredMindEffect>().ExtraDamage += CombatRef.SpiritLevel;
            }
        }
        orig(self);
    }
}
