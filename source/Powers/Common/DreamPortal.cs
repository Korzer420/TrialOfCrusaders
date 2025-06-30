using HutongGames.PlayMaker.Actions;
using KorzUtils.Helper;
using System.Collections;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.UnityComponents.Other;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class DreamPortal : Power
{
    private Coroutine _coroutine;
    private GameObject _dreamGatePrefab;
    private GameObject _activeDreamGate;

    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    public override DraftPool Pools => DraftPool.Endurance | DraftPool.Ability;

    protected override void Enable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        _dreamGatePrefab = HeroController.instance.gameObject.LocateMyFSM("Dream Nail")
            .GetState("Spawn Gate")
            .GetFirstAction<SpawnObjectFromGlobalPool>().gameObject.Value;
    }

    protected override void Disable() => UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        if (arg1.name == "Crossroads_08" || arg1.name == "Fungus1_32" 
            || arg1.name == "Fungus2_05" || arg1.name == "Ruins1_05"
            || arg1.name == "Ruins2_09" || arg1.name == "Deepnest_33"
            || arg1.name == "Fungus3_05" || arg1.name == "Fungus3_10"
            || arg1.name == "Room_Fungus_Shaman")
            return;
        GameObject holder = new("Dream Portal");
        holder.AddComponent<Dummy>().StartCoroutine(CheckForInput());
    }

    private IEnumerator CheckForInput()
    {
        float holdTime = 0;
        int amountPressed = 0;
        float repeatTime = 0f;
        float cooldown = 0f;
        while (true)
        {
            if (HeroController.instance?.acceptingInput != true)
                yield return new WaitUntil(() => HeroController.instance?.acceptingInput == true);
            if (InputHandler.Instance.inputActions.quickMap.WasPressed && cooldown <= 0f)
            {
                amountPressed++;
                repeatTime = 0.5f;
                holdTime = 0f;
            }
            else if (InputHandler.Instance.inputActions.quickMap.IsPressed && cooldown <= 0f)
                holdTime += Time.deltaTime;
            else
                holdTime = 0f;
            if (holdTime >= 1.5f)
            {
                holdTime = 0f;
                // Spawn dream portal.
                if (_activeDreamGate != null)
                    GameObject.Destroy(_activeDreamGate);
                _activeDreamGate = GameObject.Instantiate(_dreamGatePrefab);
                _activeDreamGate.transform.position = HeroController.instance.transform.localPosition - new Vector3(0f, 1.4f);
            }
            else if (amountPressed >= 3)
            {
                amountPressed = 0;
                if (_activeDreamGate != null)
                {
                    HeroController.instance.transform.position = _activeDreamGate.transform.position + new Vector3(0f, 1.4f);
                    cooldown = 15;
                    foreach (SpriteRenderer spriteRenderer in _activeDreamGate.GetComponentsInChildren<SpriteRenderer>(true))
                        spriteRenderer.color = Color.green;
                }
            }
            if (repeatTime >= 0)
                repeatTime -= Time.deltaTime;
            if (cooldown > 0)
            {
                cooldown -= Time.deltaTime;
                if (cooldown <= 0f)
                {
                    cooldown = 0f;
                    if (_activeDreamGate != null)
                        foreach (SpriteRenderer spriteRenderer in _activeDreamGate.GetComponentsInChildren<SpriteRenderer>(true))
                            spriteRenderer.color = Color.white;
                }
            }

            yield return null;
        }
    }
}
