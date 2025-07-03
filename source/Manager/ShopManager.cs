using TrialOfCrusaders.UnityComponents.Other;
using UnityEngine;

namespace TrialOfCrusaders.Manager;

internal static class ShopManager
{
    private static ShopStock _currentStock;

    public static GameObject Tuk { get; set; }

    /*
     Funktionweise:
     - Shop Interface erstellen (on the fly).
     - Shop Inhalt generieren (Muss für den Rest der Szene behalten werden).
     - Kaufinteraktion + UI Update hinzufügen.
     - Restock System hinzufügen (Reroll: Leere Plätze bleiben leer, Preise gehen hoch).
     
     */
    internal static void PrepareShopScene()
    {
        UnityEngine.Object.Destroy(GameObject.Find("Godseeker EngineRoom NPC"));
        var tuk = GameObject.Instantiate(ShopManager.Tuk);
        tuk.name = "Tuk Shop";
        tuk.transform.position = new(86.7f, 18.8f, 0.1f);
        tuk.SetActive(true);
        tuk.GetComponent<tk2dSprite>().FlipX = true;
        PlayMakerFSM fsm = tuk.LocateMyFSM("npc_control");
        fsm.FsmVariables.FindFsmBool("Hero Always Left").Value = false;
        fsm.FsmVariables.FindFsmBool("Hero Always Right").Value = true;

        GameObject.Destroy(GameObject.Find("final_boss_chain (4)"));
        GameObject.Destroy(GameObject.Find("final_boss_chain (5)"));

        _currentStock = tuk.AddComponent<ShopStock>();
    }

}
