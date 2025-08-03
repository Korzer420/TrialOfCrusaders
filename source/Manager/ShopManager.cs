using HutongGames.PlayMaker.Actions;
using KorzUtils.Data;
using KorzUtils.Helper;
using TrialOfCrusaders.UnityComponents.Other;
using UnityEngine;
using static TrialOfCrusaders.ControllerShorthands;

namespace TrialOfCrusaders.Manager;

internal static class ShopManager
{
    public static GameObject Tuk { get; set; }

    internal static void PrepareShopScene()
    {
        UnityEngine.Object.Destroy(GameObject.Find("Godseeker EngineRoom NPC"));
        var tuk = GameObject.Instantiate(Tuk);
        tuk.name = "Tuk Shop";
        tuk.transform.position = new(86.7f, 18.8f, 0.1f);
        tuk.SetActive(true);
        tuk.GetComponent<tk2dSprite>().FlipX = true;
        PlayMakerFSM fsm = tuk.LocateMyFSM("npc_control");
        fsm.FsmVariables.FindFsmBool("Hero Always Left").Value = false;
        fsm.FsmVariables.FindFsmBool("Hero Always Right").Value = true;

        GameObject.Destroy(GameObject.Find("final_boss_chain (4)"));
        GameObject.Destroy(GameObject.Find("final_boss_chain (5)"));

        tuk.AddComponent<ShopStock>();
    }

    internal static void ModifyTuk(PlayMakerFSM self)
    {
        self.AddState("Show Shop", () =>
        {
            SaveManager.CurrentSaveData.EncounteredTuk = true;
            self.GetComponent<ShopStock>().GenerateShopUI();
        }, FsmTransitionData.FromTargetState("Talk Finish").WithEventName("CONVO_FINISH"));

        bool upgrade = SecretRef.TriggerShopUpgrade();
        if (upgrade)
            SaveManager.CurrentSaveData.EncounteredTuk = false;
        if (!SaveManager.CurrentSaveData.EncounteredTuk)
        {
            self.AddState("Check Intro", () =>
            {
                if (SaveManager.CurrentSaveData.EncounteredTuk)
                    self.SendEvent("FINISHED");
                else
                    self.SendEvent("MEET");
            }, FsmTransitionData.FromTargetState("Box Up").WithEventName("MEET"),
            FsmTransitionData.FromTargetState("Show Shop").WithEventName("FINISHED"));
            self.GetState("Title").AdjustTransitions("Check Intro");
            self.GetState("Convo Choice").AdjustTransitions("Meet");
            self.GetState("Box Down 2").AdjustTransitions("Show Shop");
            self.GetState("Show Shop").AdjustTransitions("Box Up 2");
            if (upgrade)
                self.GetState("Meet").GetFirstAction<CallMethodProper>().parameters[0].stringValue = SecretRef.ShopLevel == 4 
                    ? "TUK_Final_Upgrade"
                    : "TUK_Upgrade";
        }
        else
            self.GetState("Title").AdjustTransitions("Show Shop");
    }

}
