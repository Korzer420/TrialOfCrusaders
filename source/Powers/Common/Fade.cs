﻿using KorzUtils.Helper;
using Modding;
using System.Collections;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class Fade : Power
{
    private bool _fading;
    private Coroutine _routine;

    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    public override DraftPool Pools => DraftPool.Endurance;

    protected override void Enable()
    {
        _routine = StartRoutine(WaitForFade());
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
    }

    protected override void Disable()
    {
        if (_routine != null)
            StopRoutine(_routine);
        ModHooks.GetPlayerBoolHook -= ModHooks_GetPlayerBoolHook;
    }

    private bool ModHooks_GetPlayerBoolHook(string name, bool orig)
    {
        if (name == nameof(PlayerData.isInvincible))
            return orig || _fading;
        return orig;
    }

    private IEnumerator WaitForFade()
    {
        while (true)
        {
            float passedTime = 0f;
            while (passedTime < 5f)
            {
                yield return null;
                passedTime += Time.deltaTime;
                if (HeroController.instance.hero_state != GlobalEnums.ActorStates.idle || InputHandler.Instance.inputActions.attack.IsPressed
                    || InputHandler.Instance.inputActions.dash.IsPressed || InputHandler.Instance.inputActions.superDash.IsPressed
                    || InputHandler.Instance.inputActions.dreamNail.IsPressed || InputHandler.Instance.inputActions.quickCast.IsPressed
                    || InputHandler.Instance.inputActions.focus.IsPressed || InputHandler.Instance.inputActions.cast.IsPressed
                    || InputHandler.Instance.inputActions.quickMap.IsPressed || !HeroController.instance.acceptingInput)
                    break;
                if (StageController.CurrentRoom.BossRoom || GameManager.instance.IsGamePaused())
                    passedTime = 0f;
            }
            if (passedTime >= 5f)
            {
                Color color = HeroHelper.Sprite.color;
                color.a = 0.25f;
                HeroHelper.Sprite.color = color;
                _fading = true;
            }
            else
            {
                HeroHelper.Sprite.color = Color.white;
                _fading = false;
            }
        }
    }
}