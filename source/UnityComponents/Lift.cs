using IL.InControl.UnityDeviceProfiles;
using KorzUtils.Helper;
using Modding;
using System.Collections;
using UnityEngine;

namespace TrialOfCrusaders.UnityComponents;

public class Lift : MonoBehaviour
{
    private bool _sequenceActive;

    public Lift Partner { get; set; }

    public float Cooldown { get; set; } = 0f;

    void Start()
    {
        gameObject.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Other.Lift");
        gameObject.AddComponent<BoxCollider2D>().size = new(2f, 2f);
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    void Update()
    {
        if (Cooldown > 0 && !_sequenceActive)
            Cooldown -= Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && Cooldown <= 0f && Partner.Cooldown <= 0f)
            StartCoroutine(MoveSequence());
    }

    private IEnumerator MoveSequence()
    {
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
        Cooldown = 3f;
        _sequenceActive = true;
        PDHelper.DisablePause = true;
        HeroController.instance.RelinquishControl();
        HeroController.instance.AffectedByGravity(false);

        CameraLockArea[] locks = GameObject.FindObjectsOfType<CameraLockArea>();
        foreach (var cameraLock in locks)
            cameraLock.gameObject.SetActive(false);
        CameraController controller = Object.FindObjectOfType<CameraController>();
        controller.camTarget.mode = CameraTarget.TargetMode.FOLLOW_HERO;

        Vector3 currentPosition = HeroController.instance.transform.position;
        HeroHelper.Sprite.color = new(1f,1f,1f,0f);
        while(Vector3.Distance(currentPosition, Partner.transform.position) > 0.1f)
        {
            currentPosition = Vector3.MoveTowards(currentPosition, Partner.transform.position, Time.deltaTime * 50);
            HeroController.instance.transform.position = currentPosition;
            yield return null;
        }
        HeroController.instance.transform.position = Partner.transform.position;
        yield return new WaitForSeconds(1f);
        _sequenceActive = false;
        HeroController.instance.AffectedByGravity(true);
        HeroHelper.Sprite.color = new(1f, 1f, 1f, 1f);
        HeroController.instance.RegainControl();
        PDHelper.DisablePause = false;
        Partner.Cooldown = 3f;
        ModHooks.GetPlayerBoolHook -= ModHooks_GetPlayerBoolHook;
        foreach (var cameraLock in locks)
            cameraLock.gameObject.SetActive(true);
    }

    private bool ModHooks_GetPlayerBoolHook(string name, bool orig)
    {
        if (name == nameof(PlayerData.isInvincible))
            return true;
        return orig;
    }
}
