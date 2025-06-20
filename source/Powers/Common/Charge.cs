using System.Collections;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class Charge : Power
{
    private Coroutine _coroutine;
    
    public override string Name => "CHAAARGE!!!";

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    public override StatScaling Scaling => StatScaling.Combat;

    public override DraftPool Pools => DraftPool.Combat | DraftPool.Burst;

    public bool Active { get; set; }

    protected override void Enable() => UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

    protected override void Disable() => UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;

    private IEnumerator ChargeBuff()
    {
        Active = true;
        float passedTime = 0f;
        while(passedTime <= 5 + CombatController.CombatLevel / 2)
        {
            passedTime+= Time.deltaTime;
            yield return null;
        }
        Active = false;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        if (_coroutine != null)
            StopRoutine(_coroutine);
        _coroutine = StartRoutine(ChargeBuff());
    }
}