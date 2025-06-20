using HutongGames.PlayMaker;
using KorzUtils.Helper;
using System;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;
using TrialOfCrusaders.Powers.Common;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Uncommon;

public class ImprovedWeaversong : Power
{
    public override (float, float, float) BonusRates => new(35f, 0f, 5f);

    public override Rarity Tier => Rarity.Uncommon;

    public override bool CanAppear => HasPower<Weaversong>();

    public override StatScaling Scaling => StatScaling.Combat;

    public override DraftPool Pools => DraftPool.Charm | DraftPool.Upgrade | DraftPool.Combat;

    protected override void Enable()
    {
        CharmHelper.EnsureEquipCharm(KorzUtils.Enums.CharmRef.Weaversong);
        On.HutongGames.PlayMaker.Actions.SetScale.OnEnter += SetScale_OnEnter;
        On.HutongGames.PlayMaker.Actions.Tk2dPlayAnimation.OnEnter += Tk2dPlayAnimation_OnEnter;
        On.SetHP.OnEnter += SetHP_OnEnter;
    }

    protected override void Disable()
    {
        On.HutongGames.PlayMaker.Actions.SetScale.OnEnter -= SetScale_OnEnter;
        On.HutongGames.PlayMaker.Actions.Tk2dPlayAnimation.OnEnter -= Tk2dPlayAnimation_OnEnter;
        On.SetHP.OnEnter -= SetHP_OnEnter;
    }

    private void ModifyWeaverSize(FsmStateAction self)
    {
        float weaverScale = 1.1f + (Math.Min(0.9f, CombatController.CombatLevel * 0.05f));

        self.Fsm.Variables.FindFsmFloat("Scale").Value = weaverScale;
        self.Fsm.Variables.FindFsmFloat("Neg Scale").Value = weaverScale * -1f;
        self.Fsm.FsmComponent.transform.localScale = new Vector3(weaverScale, weaverScale);
        self.Fsm.FsmComponent.transform.SetScaleMatching(weaverScale);
    }

    private void Tk2dPlayAnimation_OnEnter(On.HutongGames.PlayMaker.Actions.Tk2dPlayAnimation.orig_OnEnter orig, HutongGames.PlayMaker.Actions.Tk2dPlayAnimation self)
    {
        orig(self);
        if (self.IsCorrectContext("Control", null, "Run Dir") && self.Fsm.FsmComponent.gameObject.name.Contains("Weaverling"))
            ModifyWeaverSize(self);
    }

    private void SetScale_OnEnter(On.HutongGames.PlayMaker.Actions.SetScale.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetScale self)
    {
        orig(self);
        if (self.IsCorrectContext("Control", null, null) && self.Fsm.FsmComponent.gameObject.name.Contains("Weaverling"))
            ModifyWeaverSize(self);
    }

    private void SetHP_OnEnter(On.SetHP.orig_OnEnter orig, SetHP self)
    {
        try
        {
            if (self.IsCorrectContext("Attack", "Enemy Damager", "Hit") && 
                self.Fsm.GameObject.transform.parent != null && self.Fsm.GameObject.transform.parent.name.StartsWith("Weaverling"))
                self.hp.Value -= 2 + Mathf.FloorToInt(CombatController.CombatLevel * 2.5f);
        }
        catch (Exception ex)
        {
            LogManager.Log("failed to modify improved weaversong damage. ",ex);
        }
        orig(self);
    }
}
