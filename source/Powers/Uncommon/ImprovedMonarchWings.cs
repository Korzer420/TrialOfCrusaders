using KorzUtils.Helper;
using Modding.Utils;
using System.Collections;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.UnityComponents.CombatElements;
using TrialOfCrusaders.UnityComponents.Debuffs;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ImprovedMonarchWings : Power
{
    private GameObject _wings;

    private GameObject _leftHitbox;

    private GameObject _rightHitbox;

    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public override bool CanAppear => PDHelper.HasDoubleJump;

    public override StatScaling Scaling => StatScaling.Combat;

    public override DraftPool Pools => DraftPool.Combat | DraftPool.Ability | DraftPool.Upgrade | DraftPool.Debuff;

    protected override void Enable()
    {
        _wings = HeroController.instance.transform.Find("Effects/Double J Wings").gameObject;
        if (_leftHitbox != null)
            GameObject.Destroy(_leftHitbox);
        _leftHitbox = GameObject.Instantiate(HeroController.instance.transform.Find("Charm Effects/Thorn Hit/Hit L").gameObject, _wings.transform);
        _leftHitbox.transform.localPosition = new(-1.0555f, -0.6111f, 0f);
        _leftHitbox.transform.localScale = new(3.5f, 2.3f);
        _leftHitbox.name = "Wing Hitbox";
        Component.Destroy(_leftHitbox.GetComponent<PolygonCollider2D>());
        _leftHitbox.AddComponent<BoxCollider2D>().isTrigger = true;
        Component.Destroy(_leftHitbox.LocateMyFSM("set_thorn_damage"));
        PlayMakerFSM fsm = _leftHitbox.LocateMyFSM("damages_enemy");
        fsm.FsmVariables.FindFsmInt("damageDealt").Value = 50 + CombatRef.CombatLevel * 2;
        fsm.FsmVariables.FindFsmFloat("magnitudeMult").Value = 4;
        fsm.FsmVariables.FindFsmFloat("direction").Value = 270;

        // Check for terrain
        fsm.GetState("Send Event").InsertActions(2, () =>
        {
            if (fsm.FsmVariables.FindFsmInt("Layer").Value == 8)
                fsm.SendEvent("CANCEL");
        });

        if (_rightHitbox != null)
            GameObject.Destroy(_rightHitbox);
        _rightHitbox = GameObject.Instantiate(HeroController.instance.transform.Find("Charm Effects/Thorn Hit/Hit R").gameObject, _wings.transform);
        _rightHitbox.transform.localPosition = new(1.02f, -0.6111f);
        _rightHitbox.transform.localScale = new(3.5f, 2.3f);
        _rightHitbox.name = "Wing Hitbox";
        Component.Destroy(_rightHitbox.GetComponent<PolygonCollider2D>());
        _rightHitbox.AddComponent<BoxCollider2D>().isTrigger = true;
        Component.Destroy(_rightHitbox.LocateMyFSM("set_thorn_damage"));
        fsm = _rightHitbox.LocateMyFSM("damages_enemy");
        fsm.FsmVariables.FindFsmInt("damageDealt").Value = 50 + CombatRef.CombatLevel * 2;
        fsm.FsmVariables.FindFsmFloat("magnitudeMult").Value = 4;
        fsm.FsmVariables.FindFsmFloat("direction").Value = 270;

        // Check for terrain
        fsm.GetState("Send Event").InsertActions(2, () =>
        {
            if (fsm.FsmVariables.FindFsmInt("Layer").Value == 8)
                fsm.SendEvent("CANCEL");
        });

        _leftHitbox.gameObject.SetActive(false);
        _rightHitbox.gameObject.SetActive(false);

        On.HeroController.DoDoubleJump += HeroController_DoDoubleJump;
        On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter += TakeDamage_OnEnter;
    }

    protected override void Disable()
    {
        GameObject.Destroy(_leftHitbox);
        GameObject.Destroy(_rightHitbox);
        On.HeroController.DoDoubleJump -= HeroController_DoDoubleJump;
        On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter -= TakeDamage_OnEnter;
    }

    private IEnumerator ActivateHitbox()
    {
        if (_leftHitbox == null)
        { 
            Disable();
            Enable();
        }
        _leftHitbox.SetActive(true);
        _rightHitbox.SetActive(true);
        yield return new WaitForSeconds(.15f);
        _leftHitbox.SetActive(false);
        _rightHitbox.SetActive(false);
    }

    private void HeroController_DoDoubleJump(On.HeroController.orig_DoDoubleJump orig, HeroController self)
    {
        orig(self);
        HeroController.instance.StartCoroutine(ActivateHitbox());
    }

    private void TakeDamage_OnEnter(On.HutongGames.PlayMaker.Actions.TakeDamage.orig_OnEnter orig, HutongGames.PlayMaker.Actions.TakeDamage self)
    {
        orig(self);
        if (self.IsCorrectContext("damages_enemy", "Wing Hitbox", null) && self.Target.Value.GetComponent<HealthManager>())
        {
            if (self.Target.Value.GetComponent<EnemyShield>() == null)
            {
                if (Random.Range(1f, 100f) <= 50)
                {
                    if (Random.Range(0, 2) == 0)
                        self.Target.Value.AddComponent<BleedEffect>();
                    else
                        self.Target.Value.GetOrAddComponent<ConcussionEffect>().Timer += 3;
                }
                self.Target.Value.AddComponent<EnemyShield>()
                    .SetTimer(5f);
            }
        }
    }
}
