using KorzUtils.Helper;
using System.Collections;
using System.Collections.Generic;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;
using TrialOfCrusaders.UnityComponents.Other;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class LifebloodOmen : Power
{
    private Coroutine _coroutine;

    public override (float, float, float) BonusRates => new(5f, 0f, 5f);

    public override StatScaling Scaling => StatScaling.Spirit; 

    public static List<GameObject> Ghosts { get; set; } = [];

    protected override void Enable()
    {
        CombatController.BeginCombat += CombatController_BeginCombat;
    }

    protected override void Disable() => CombatController.BeginCombat -= CombatController_BeginCombat;

    private void CombatController_BeginCombat()
    {
        GameObject gameObject = new("Haunt");
        gameObject.SetActive(true);
        gameObject.AddComponent<Dummy>().StartCoroutine(Haunt());
    }

    private IEnumerator Haunt()
    {
        float delay = UnityEngine.Random.Range(90, 301);
        while(delay > 0)
        {
            delay -= Time.deltaTime;
            yield return null;
            if (GameManager.instance.IsGamePaused())
                yield return new WaitUntil(() => !GameManager.instance.IsGamePaused());
        }    
        GameObject ghost;
        int index = DetermineGhost();
        ghost = GameObject.Instantiate(Ghosts[index]);
        if (ghost.GetComponent<PersistentBoolItem>() is PersistentBoolItem boolItem)
            Component.Destroy(boolItem);
        ghost.transform.localPosition = HeroController.instance.transform.localPosition + new Vector3(0f, 3f, 0f);
        ghost.name = "Lifeblood Ghost";
        ghost.GetComponent<tk2dSprite>().color = Color.cyan;
        PlayMakerFSM fsm = ghost.LocateMyFSM("Control");
        fsm.GetState("Set Level").RemoveActions(0);
        fsm.GetState("Set Level").AddActions(() =>
        {
            fsm.FsmVariables.FindFsmInt("Grimmchild Level").Value = 1;
            fsm.SendEvent("LEVEL 1");
        });
        // Prevent grimm music playing.
        fsm.GetState("Alert Pause").RemoveTransitionsTo("Music");
        fsm.GetState("Alert Pause").AddTransition("FINISHED", "Set Angle");

        // Remove accordion fanfare
        fsm.GetState("Fanfare 1").RemoveActions(0);

        // Reward
        fsm.GetState("Explode").ReplaceAction(4, () =>
        {
            fsm.FsmVariables.FindFsmGameObject("Explode Effects").Value.SetActive(true);
            for (int i = 0; i < 3 * (index + 1); i++)
                EventRegister.SendEvent("ADD BLUE HEALTH");
        });
        fsm.SendEvent("START");

        float activeTime = 0f;
        float duration = RngManager.GetRandom(30f, 90f);
        while (activeTime < duration)
        {
            activeTime += Time.deltaTime;
            yield return null;
            if (GameManager.instance.IsGamePaused())
                yield return new WaitUntil(() => !GameManager.instance.IsGamePaused());
        }
        if (ghost != null)
        {
            PlayMakerFSM.BroadcastEvent("DREAM AREA DISABLE");
            GameObject.Destroy(ghost);
        }
    }

    private int DetermineGhost()
    {
        switch (CombatController.SpiritLevel / 5)
        {
            case 0:
                // 20% chance for medium ghost.
                return Random.Range(0, 5) == 0 ? 1 : 0;
            case 1:
                // 40 % chance for medium ghost.
                return Random.Range(0, 5) <= 1 ? 1 : 0;
            case 2:
                // 50 % chance for medium, 35 % for small and 15 % for large ghost.
                int result = Random.Range(1, 21);
                return result <= 10 ? 1 : (result <= 17 ? 0 : 2);
            default:
                // 55 % for medium, 35% chance for large and 10% for small ghost.
                int highRoll = Random.Range(1, 21);
                return highRoll <= 7 ? 2 : (highRoll <= 18 ? 1 : 0);
        }
    }
}
