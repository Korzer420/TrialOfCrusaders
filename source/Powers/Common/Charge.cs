using KorzUtils.Helper;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class Charge : Power
{
    private bool _charge;
    private Coroutine _coroutine;
    private ILHook _attackMethod;

    public override string Name => "CHAAARGE!!!";

    public override string Description => "You move and attack faster shortly after entering the room.";

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    internal override void Enable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        IL.HeroController.Move += HeroController_Move;
        _attackMethod = new(typeof(HeroController).GetMethod("orig_DoAttack", BindingFlags.NonPublic | BindingFlags.Instance), ModifyAttackSpeed);
    }

    private void HeroController_Move(MonoMod.Cil.ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.Goto(0);
        cursor.GotoNext(MoveType.After, x => x.MatchLdfld<HeroController>(nameof(HeroController.RUN_SPEED_CH_COMBO)));
        cursor.EmitDelegate<Func<float, float>>(x => _charge ? x * 2 : x);
        cursor.GotoNext(MoveType.After, x => x.MatchLdfld<HeroController>(nameof(HeroController.RUN_SPEED_CH)));
        cursor.EmitDelegate<Func<float, float>>(x => _charge ? x * 2 : x);
        cursor.GotoNext(MoveType.After, x => x.MatchLdfld<HeroController>(nameof(HeroController.RUN_SPEED)));
        cursor.EmitDelegate<Func<float, float>>(x => _charge ? x * 2 : x);
    }

    internal override void Disable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        IL.HeroController.Move -= HeroController_Move;
        _attackMethod?.Dispose();
    }

    private IEnumerator ChargeBuff()
    {
        _charge = true;
        float passedTime = 0f;
        while(passedTime <= 10f)
        {
            passedTime+= Time.deltaTime;
            yield return null;
        }
        _charge = false;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        if (_coroutine != null)
            StopRoutine(_coroutine);
        _coroutine = StartRoutine(ChargeBuff());
    }

    private void ModifyAttackSpeed(ILContext context)
    {
        ILCursor cursor = new(context);
        cursor.Goto(0);
        cursor.GotoNext(MoveType.After, x => x.MatchLdfld<HeroController>(nameof(HeroController.ATTACK_COOLDOWN_TIME_CH)));
        cursor.EmitDelegate<Func<float, float>>(x => _charge ? x / 2 : x);
        cursor.GotoNext(MoveType.After, x => x.MatchLdfld<HeroController>(nameof(HeroController.ATTACK_COOLDOWN_TIME)));
        // 10 stacks should match quick slash.
        cursor.EmitDelegate<Func<float, float>>(x => _charge ? x / 2 : x);
    }
}