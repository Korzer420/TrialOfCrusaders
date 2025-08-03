using HutongGames.PlayMaker.Actions;
using KorzUtils.Helper;
using System;
using TMPro;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using UnityEngine;

namespace TrialOfCrusaders.Controller;

public class ConsumableController : BaseController
{
    private TMP_Text _freeSpellCounter;
    private int _teaSpell = 0;

    public int UsedEggs { get; set; }

    public int UsedLifeblood { get; set; }

    public int TeaSpell
    {
        get => _teaSpell;
        set
        {
            if (value == 0)
            {
                if (_freeSpellCounter != null)
                    GameObject.Destroy(_freeSpellCounter.gameObject);
                _freeSpellCounter = null;
            }
            else 
            {
                if (_freeSpellCounter == null)
                {
                    GameObject prefab = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Inv_Items/Geo/Geo Amount").gameObject;
                    Transform parent = GameObject.Find("_GameCameras").transform.Find("HudCamera/Hud Canvas");
                    GameObject cover = GameObject.Instantiate(prefab, parent, true);
                    if (cover.GetComponent<DisplayItemAmount>() is DisplayItemAmount amount)
                        Component.Destroy(amount);
                    cover.transform.position = new(-12.41f, 6.2f, 0.1093f);
                    cover.transform.localScale = new(1.2369f, 1.2369f, 0.751f);
                    _freeSpellCounter = cover.GetComponent<TMP_Text>();
                    cover.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;
                    _freeSpellCounter.color = Color.cyan;
                    _freeSpellCounter.fontSize = 8;
                }
                _freeSpellCounter.text = value.ToString();
            }
            _teaSpell = value;
        }
    }

    public int EmpoweredHits { get; set; }

    public int RerollSeals { get; set; }

    public int EggHeal => Math.Max(5, 25 - 5 * UsedEggs);

    public int LifebloodHeal => 3 + UsedLifeblood * 2;

    public override Phase[] GetActivePhases() => [Phase.Run];

    protected override void Enable()
    {
        On.HeroController.DoAttack += HeroController_DoAttack;
        On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += IntCompare_OnEnter;
        On.HutongGames.PlayMaker.Actions.SendMessage.OnEnter += SendMessage_OnEnter;
    }

    protected override void Disable()
    {
        On.HeroController.DoAttack -= HeroController_DoAttack;
        On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter -= IntCompare_OnEnter;
        On.HutongGames.PlayMaker.Actions.SendMessage.OnEnter -= SendMessage_OnEnter;
        TeaSpell = 0;
        UsedLifeblood = 0;
        UsedEggs = 0;
        EmpoweredHits = 0;
        RerollSeals = 0;
    }

    private void HeroController_DoAttack(On.HeroController.orig_DoAttack orig, HeroController self)
    {
        orig(self);
        if (EmpoweredHits > 0)
        {
            EmpoweredHits = EmpoweredHits.Lower(1);
            if (EmpoweredHits == 0)
            { 
                PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
                GameObject nail;
                var state = HeroController.instance.transform.Find("Hero Death").gameObject.LocateMyFSM("Hero Death Anim").GetState("Blow");
                if (state.GetFirstAction<CreateObject>() is CreateObject createAction)
                    nail = GameObject.Instantiate(createAction.gameObject.Value);
                else
                    nail = GameObject.Instantiate(state.GetFirstAction<FlingObjectsFromGlobalPool>().gameObject.Value);

                nail.transform.position = HeroController.instance.transform.position;
                nail.GetComponent<Rigidbody2D>().velocity = new(20f, 20f);
                nail.GetComponent<Rigidbody2D>().AddTorque(100f);
                nail.SetActive(true);
            }
        }
    }

    private void IntCompare_OnEnter(On.HutongGames.PlayMaker.Actions.IntCompare.orig_OnEnter orig, HutongGames.PlayMaker.Actions.IntCompare self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Can Cast? QC") && TeaSpell > 0)
            self.integer1.Value = self.integer2.Value;
        orig(self);
    }

    private void SendMessage_OnEnter(On.HutongGames.PlayMaker.Actions.SendMessage.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SendMessage self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", null) && TeaSpell > 0 && self.functionCall.FunctionName == "TakeMP")
        {
            int cost = self.functionCall.IntParameter.Value;
            TeaSpell--;
            self.functionCall.IntParameter.Value = 0;
            orig(self);
            self.functionCall.IntParameter.Value = cost;
        }
        else
            orig(self);
    }
}
