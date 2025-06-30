using KorzUtils.Helper;
using System.Collections;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class Damocles : Power
{
    private Coroutine _coroutine;

    public bool Triggered { get; set; }

    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override DraftPool Pools => DraftPool.Risk | DraftPool.Treasure;

    protected override void Enable()
    {
        if (Triggered)
            _coroutine = StartRoutine(Tick());
        CombatController.TookDamage += CombatController_TookDamage;
    }

    protected override void Disable()
    {
        if (_coroutine != null)
            StopRoutine(_coroutine);
        CombatController.TookDamage -= CombatController_TookDamage;
    }

    private void CombatController_TookDamage()
    {
        if (_coroutine == null)
        {
            Triggered = true;
            GameManager.instance.SaveGame();
            _coroutine = StartRoutine(Tick());
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
        yield return new WaitUntil(() => !PDHelper.IsInvincible && HeroController.instance?.acceptingInput == true);
        GameHelper.DisplayMessage("Your time is up...");
        HeroController.instance.TakeDamage(HeroController.instance.gameObject, GlobalEnums.CollisionSide.bottom, CombatController.InstaKillDamage, 1);
    }
}