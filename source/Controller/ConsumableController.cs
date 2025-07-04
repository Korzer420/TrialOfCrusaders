using HutongGames.PlayMaker.Actions;
using KorzUtils.Helper;
using System;
using TMPro;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Manager;
using UnityEngine;

namespace TrialOfCrusaders.Controller;

internal static class ConsumableController
{
    private static bool _enabled;
    private static TMP_Text _freeSpellCounter;
    private static int _teaSpell = 0;

    public static int UsedEggs { get; set; }

    public static int UsedLifeblood { get; set; }

    public static int TeaSpell
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

    public static int EmpoweredHits { get; set; }

    public static int RerollSeals { get; set; }

    public static int EggHeal => Math.Max(5, 25 - 5 * UsedEggs);

    public static int LifebloodHeal => Math.Min(3, 3 + UsedLifeblood * 2);

    internal static void Initialize()
    {
        if (_enabled)
            return;
        On.HeroController.DoAttack += HeroController_DoAttack;
        On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += IntCompare_OnEnter;
        On.HutongGames.PlayMaker.Actions.SendMessage.OnEnter += SendMessage_OnEnter;

        _enabled = true;
    }

    internal static void Unload()
    {
        if (!_enabled)
            return;
        On.HeroController.DoAttack -= HeroController_DoAttack;
        On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter -= IntCompare_OnEnter;
        On.HutongGames.PlayMaker.Actions.SendMessage.OnEnter -= SendMessage_OnEnter;
        TeaSpell = 0;
        _enabled = false;
    }

    private static void HeroController_DoAttack(On.HeroController.orig_DoAttack orig, HeroController self)
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

    private static void IntCompare_OnEnter(On.HutongGames.PlayMaker.Actions.IntCompare.orig_OnEnter orig, HutongGames.PlayMaker.Actions.IntCompare self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Can Cast? QC") && TeaSpell > 0)
            self.integer1.Value = self.integer2.Value;
        orig(self);
    }

    private static void SendMessage_OnEnter(On.HutongGames.PlayMaker.Actions.SendMessage.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SendMessage self)
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
