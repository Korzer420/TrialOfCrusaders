using KorzUtils.Enums;
using KorzUtils.Helper;
using Modding;
using System.Collections;
using TrialOfCrusaders.UnityComponents;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class Damocles : Power
{
    private Coroutine _coroutine;

    private GameObject _activeSoulCache;

    public static GameObject SoulCache { get; set; }

    public bool Triggered { get; set; }

    public override string Name => "Damocles";

    public override string Description => "Increases the chance of treasure room significantly. After taking damage the first time, each second could be your last...";

    public override (float, float, float) BonusRates => new(0f, 10f, 0f);

    protected override void Enable()
    {
        On.HeroController.TakeDamage += HeroController_TakeDamage;
        if (Triggered)
            _coroutine = StartRoutine(Tick());
    }

    protected override void Disable()
    {
        On.HeroController.TakeDamage -= HeroController_TakeDamage;
        if (_coroutine != null)
            StopRoutine(_coroutine);
    }

    private void HeroController_TakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
    {
        int currentHealth = PDHelper.Health;
        orig(self, go, damageSide, damageAmount, hazardType);
        if (currentHealth != PDHelper.Health && _coroutine == null)
        {
            _coroutine = StartRoutine(Tick());
            Triggered = true;
        }
    }

    private IEnumerator Tick()
    {
        bool triggered = false;
        while (!triggered)
        {
            yield return new WaitForSeconds(1f);
            if (HeroController.instance?.acceptingInput != true)
                yield return new WaitUntil(() => HeroController.instance == null || HeroController.instance.acceptingInput);
            if (HeroController.instance == null)
                yield break;
            triggered = Random.Range(1, 1201) == 1;
        }
        HeroController.instance.TakeDamage(HeroController.instance.gameObject, GlobalEnums.CollisionSide.bottom, 500, 1);
    }
}