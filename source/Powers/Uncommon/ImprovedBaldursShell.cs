using HutongGames.PlayMaker.Actions;
using KorzUtils.Helper;
using Modding;
using System;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ImprovedBaldursShell : Power
{
    private int _currentHitPoints = 10;

    private int _passedScenes = 0;

    public override (float, float, float) BonusRates => new(0f, 0f, 40f);

    public override Rarity Tier => Rarity.Uncommon;

    public override bool CanAppear => HasPower<BaldurShell>();

    protected override void Enable()
    {
        _currentHitPoints = 10;
        On.PlayerData.IntAdd += PlayerData_IntAdd;
        ModHooks.GetPlayerIntHook += ModHooks_GetPlayerIntHook;
        On.HutongGames.PlayMaker.Actions.IntSwitch.OnEnter += IntSwitch_OnEnter;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    protected override void Disable()
    {
        On.PlayerData.IntAdd -= PlayerData_IntAdd;
        ModHooks.GetPlayerIntHook -= ModHooks_GetPlayerIntHook;
        On.HutongGames.PlayMaker.Actions.IntSwitch.OnEnter -= IntSwitch_OnEnter;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
    }

    private void IntSwitch_OnEnter(On.HutongGames.PlayMaker.Actions.IntSwitch.orig_OnEnter orig, IntSwitch self)
    {
        if (self.IsCorrectContext("Control", "Blocker Shield", "Blocker Hit"))
            self.intVariable.Value = Math.Max(3, _currentHitPoints);
        orig(self);
    }

    private void PlayerData_IntAdd(On.PlayerData.orig_IntAdd orig, PlayerData self, string intName, int amount)
    {
        if (intName == nameof(PlayerData.blockerHits))
        { 
            _currentHitPoints += amount;
            amount = 0;
        }
        orig(self, intName, amount);
    }

    private int ModHooks_GetPlayerIntHook(string name, int orig)
    {
        if (name == nameof(PlayerData.blockerHits))
            return _currentHitPoints;
        return orig;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        _passedScenes++;
        if (_currentHitPoints < 10 && _passedScenes >= 10 - Mathf.CeilToInt((float)CombatController.EnduranceLevel / 3))
        {
            _passedScenes = 0;
            _currentHitPoints++;
        }
    }
}
