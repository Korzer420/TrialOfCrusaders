using GlobalEnums;
using KorzUtils.Helper;
using System.Collections;
using TrialOfCrusaders.Controller;
using UnityEngine;

namespace TrialOfCrusaders.UnityComponents;

/// <summary>
/// Used for a godhome transition.
/// </summary>
internal class SpecialTransition : MonoBehaviour
{
    private float _reminderTime;

    public TransitionPoint VanillaTransition { get; set; }

    public bool LoadIntoDream { get; set; }

    public bool WaitForItem { get; set; }

    void Start()
    {
        if (VanillaTransition == null)
            return;
        BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = StageController.QuietRoom;
        collider.size = VanillaTransition.GetComponent<BoxCollider2D>().size;
        transform.localScale = VanillaTransition.transform.localScale;
        VanillaTransition.GetComponent<BoxCollider2D>().enabled = false;
        transform.position = VanillaTransition.transform.position;
        if (VanillaTransition.gameObject.name.Contains("left"))
            transform.position -= new Vector3(0.6f, 0f);
        else if (VanillaTransition.gameObject.name.Contains("right"))
            transform.position += new Vector3(0.6f, 0f);
        else if (VanillaTransition.gameObject.name.Contains("top"))
            transform.position += new Vector3(0f, 0.6f);
        else if (VanillaTransition.gameObject.name.Contains("bot"))
            transform.position -= new Vector3(0f, 0.6f);
        else
            LogHelper.Write<TrialOfCrusaders>("Transition "+VanillaTransition.gameObject.name+" could not assign an offset", KorzUtils.Enums.LogType.Warning);
    }

    void Update()
    {
        _reminderTime -= Time.deltaTime;
        if (_reminderTime < 0f)
            _reminderTime = 0f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            if (!WaitForItem)
            {
                StartGodhomeTransition();
                Destroy(gameObject);
            }
            else
            {
                TrialOfCrusaders.Holder.StartCoroutine(HeroController.instance.HazardRespawn());
                if (_reminderTime <= 0f)
                {
                    _reminderTime = 5f;
                    GameHelper.DisplayMessage("You should pick up the item...");
                }
            }
        }
    }

    internal void StartGodhomeTransition()
    {
        HeroController.instance.RelinquishControl();
        HeroController.instance.AffectedByGravity(false);
        GameObject inspect = GameObject.Instantiate(StageController.TransitionObject);
        inspect.SetActive(true);
        PlayMakerFSM fsm = inspect.LocateMyFSM("GG Boss UI");
        HutongGames.PlayMaker.FsmState state = fsm.GetState("Open UI");
        state.RemoveAllActions();
        state.AdjustTransition("FINISHED", "Impact");
        fsm.GetState("Change Scene").RemoveActions(3); // Removes the enter without input command that causes endless auto walk.
        fsm.GetState("Change Scene").RemoveFirstAction<GetStaticVariable>();
        fsm.FsmVariables.FindFsmString("To Scene").Value = "Select Target";
        CoroutineHelper.WaitFrames(() => { fsm.SendEvent("CONVO START"); }, true, 1);
        if (!LoadIntoDream)
            GameManager.instance.StartCoroutine(RemoveTransitionBlocker());
    }

    private static IEnumerator RemoveTransitionBlocker()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        yield return new WaitUntil(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != currentScene);
        GameManager.instance.cameraCtrl.transform.parent.parent.Find("HudCamera/Blanker White").gameObject.LocateMyFSM("Blanker Control").SendEvent("FADE OUT");
    }
}
