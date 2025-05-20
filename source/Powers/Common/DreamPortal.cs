using HutongGames.PlayMaker.Actions;
using KorzUtils.Helper;
using System.Collections;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class DreamPortal : Power
{
    private Coroutine _coroutine;
    private GameObject _dreamGatePrefab;
    private GameObject _activeDreamGate;

    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    protected override void Enable()
    {
        _coroutine = StartRoutine(CheckForInput());
        _dreamGatePrefab = HeroController.instance.gameObject.LocateMyFSM("Dream Nail")
            .GetState("Spawn Gate")
            .GetFirstAction<SpawnObjectFromGlobalPool>().gameObject.Value;
    }

    protected override void Disable()
    {
        if (_coroutine != null)
            StopRoutine(_coroutine);
    }

    private IEnumerator CheckForInput()
    {
        float holdTime = 0;
        int amountPressed = 0;
        float repeatTime = 0f;
        float cooldown = 0f;
        while (true)
        {
            if (InputHandler.Instance.inputActions.quickMap.WasPressed && cooldown <= 0f)
            {
                amountPressed++;
                repeatTime = 0.5f;
                holdTime = 0f;
            }
            else if (InputHandler.Instance.inputActions.quickMap.IsPressed)
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
