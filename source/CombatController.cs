using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using KorzUtils.Data;
using KorzUtils.Helper;
using Modding;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TrialOfCrusaders.UnityComponents;
using UnityEngine;

namespace TrialOfCrusaders;

public static class CombatController
{
    public static string UpcomingPowerSelection { get; set; }

    /// <summary>
    /// Gets or sets the combat level.
    /// Each one increases the nail damage by two (except the last two, which grant 3 instead).
    /// </summary>
    public static int CombatLevel { get; set; }

    /// <summary>
    /// Gets or sets the spirit level.
    /// Each one increases the max amount of soul the player has by 8 (except the last two which grant 10 and 11 respectively)
    /// </summary>
    public static int SpiritLevel { get; set; }

    /// <summary>
    /// Gets or sets the survival level.
    /// Each one grants one extra mask.
    /// </summary>
    public static int EnduranceLevel { get; set; }

    public static List<HealthManager> Enemies => StageController.Enemies;

    public static List<Power> ObtainedPowers { get; set; } = [];

    public static void Initialize()
    {
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
        ModHooks.GetPlayerIntHook += ModHooks_GetPlayerIntHook;
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
        CreateExtraHealth();
        CoroutineHelper.WaitUntil(() =>
        {
            // Adjust max health and max mp.
            int intendedMaxHealth = 5 + EnduranceLevel;
            if (PDHelper.MaxHealth != intendedMaxHealth)
            {
                PDHelper.MaxHealth += intendedMaxHealth - PDHelper.MaxHealth;
                HeroController.instance.MaxHealth();
            }

            int maxMP = 33 + (Math.Min(SpiritLevel, 18) * 8) + (SpiritLevel == 19 ? 10 : 0);
            int soulVessel = 0;
            if (maxMP >= 165)
                soulVessel = 3;
            else if (maxMP > 132)
                soulVessel = 2;
            else if (maxMP > 99)
                soulVessel = 1;
            PDHelper.MPReserveMax = soulVessel * 33;
            // To not deal with the UI, we just toggle it.
            GameCameras.instance.hudCanvas.gameObject.SetActive(false);
            GameCameras.instance.hudCanvas.gameObject.SetActive(true);
        }, () => HeroController.instance?.acceptingInput == true, true);
    }

    private static void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (self.FsmName == "health_display")
        {
            self.AddState("Check for fill", () =>
            {
                int healthNumber = int.Parse(self.gameObject.name.Substring("Health ".Length));
                if (PDHelper.Health >= healthNumber)
                    self.SendEvent("HERO HEALED");
            }, FsmTransitionData.FromTargetState("Idle").WithEventName("HERO HEALED"),
               FsmTransitionData.FromTargetState("Empty").WithEventName("FINISHED"));
            FsmState state = self.GetState("Check for fill");
            if (state.Actions.Length != 4)
            {
                state.AddActions(self.GetState("Break?").GetActions<SetMeshRenderer>());
                state.AddActions(self.GetState("Break?").GetActions<Tk2dPlayAnimationWithEvents>());
                self.GetState("Max Up Flash").AdjustTransitions("Check for fill");
            }
        }
        orig(self);
    }

    public static void Unload()
    {
        ModHooks.GetPlayerIntHook -= ModHooks_GetPlayerIntHook;
        ModHooks.GetPlayerBoolHook -= ModHooks_GetPlayerBoolHook;
    }

    internal static Coroutine ExecuteRoutine(IEnumerator coroutine) => TrialOfCrusaders.Holder.StartCoroutine(coroutine);

    internal static void StopRoutine(Coroutine coroutine) => TrialOfCrusaders.Holder.StopCoroutine(coroutine);

    private static void CreateExtraHealth()
    {
        Transform cameraObject = GameObject.Find("_GameCameras").transform;
        if (cameraObject.Find("HudCamera/Hud Canvas/Health/Health 12") != null)
            return;
        GameObject healthPrefab = cameraObject.Find("HudCamera/Hud Canvas/Health/Health 11").gameObject;
        float space = healthPrefab.transform.localPosition.x - cameraObject.Find("HudCamera/Hud Canvas/Health/Health 10").localPosition.x;

        for (int i = 1; i <= 14; i++)
        {
            GameObject gameObject = GameObject.Instantiate(healthPrefab, cameraObject.Find("HudCamera/Hud Canvas/Health"));
            gameObject.name = "Health " + (i + 11);
            gameObject.LocateMyFSM("health_display").FsmVariables.FindFsmInt("Health Number").Value = i + 11;
            gameObject.transform.localPosition = new Vector3(healthPrefab.transform.localPosition.x + space * i, healthPrefab.transform.localPosition.y, healthPrefab.transform.localPosition.z);
        }
    }

    private static int ModHooks_GetPlayerIntHook(string name, int orig)
    {
        if (name == nameof(PlayerData.instance.nailDamage))
            orig = orig/*3 + (CombatLevel * 2) + (CombatLevel >= 19 ? 1 : 0)*/;
        else if (name == nameof(PlayerData.maxHealthCap))
            return 25;
        else if (name == nameof(PlayerData.maxHealthBase) || name == nameof(PlayerData.maxHealth))
            return 5 + EnduranceLevel;
        else if (name == nameof(PlayerData.maxMP))
            return Math.Min(99, 33 + (Math.Min(SpiritLevel, 18) * 8) + (SpiritLevel == 19 ? 10 : 0));
        else if (name == nameof(PlayerData.MPReserveMax))
        {
            if (SpiritLevel < 9)
                return 0;
            else if (SpiritLevel == 20)
                return 99;
            else if (SpiritLevel == 19)
                return 88;
            int spirit = Math.Max(0, -2 + (SpiritLevel - 8) * 8);
            return spirit;
        }
        return orig;
    }

    private static bool ModHooks_GetPlayerBoolHook(string name, bool orig)
    {
        // Block overcharm
        if (name == nameof(PlayerData.instance.overcharmed))
            return false;
        return orig;
    }
}
