﻿using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using KorzUtils.Data;
using KorzUtils.Helper;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;
using TrialOfCrusaders.Powers.Rare;
using UnityEngine;
using UnityEngine.UI;

namespace TrialOfCrusaders.Controller;

/// <summary>
/// Handles everything related to the score.
/// </summary>
public static class ScoreController
{
    private static bool _enabled;
    private static Coroutine _timer;
    private static int _finishedScores = 0;
    private static bool _tookDamage = false;

    #region Properties

    public static GameObject ScoreboardPrefab { get; set; }

    public static GameObject ResultSequencePrefab { get; set; }

    public static ScoreData Score { get; set; } = new();

    #endregion

    public static void Initialize()
    {
        if (_enabled)
            return;
        LogManager.Log("Enable Score Controller");
        CombatController.TookDamage += CombatController_TookDamage;
        CombatController.EnemyKilled += CombatController_EnemyKilled;
        StageController.RoomEnded += StageController_RoomEnded;
        On.HeroController.FinishedEnteringScene += HeroController_FinishedEnteringScene;
        On.PlayMakerFSM.OnEnable += SetupResultElements;
        HistoryController.CreateEntry += PassHistoryData;
        ModHooks.GetPlayerBoolHook += BlockPause;
        On.PlayerData.IncrementInt += CountGrubs;
        Score = new()
        { 
            Mode = HubController.SelectedGameMode
        };
        StartTimer();
        _enabled = true;
    }

    public static void Unload()
    {
        if (!_enabled)
            return;
        LogManager.Log("Disable Score Controller");
        CombatController.TookDamage -= CombatController_TookDamage;
        CombatController.EnemyKilled -= CombatController_EnemyKilled;
        StageController.RoomEnded -= StageController_RoomEnded;
        On.HeroController.FinishedEnteringScene -= HeroController_FinishedEnteringScene;
        On.PlayMakerFSM.OnEnable -= SetupResultElements;
        HistoryController.CreateEntry -= PassHistoryData;
        ModHooks.GetPlayerBoolHook -= BlockPause;
        On.PlayerData.IncrementInt -= CountGrubs;
        StopTimer();
        Score = null;
        _enabled = false;
    }

    #region Score Tracking

    private static void CombatController_EnemyKilled(HealthManager enemy) => Score.CurrentKillStreak++;

    private static void StageController_RoomEnded(bool quietRoom, bool traversed)
    {
        if (!quietRoom)
        {
            if (!_tookDamage)
            {
                if (StageController.CurrentRoom.BossRoom)
                    Score.PerfectBossesBonus++;
                if (StageController.CurrentRoomNumber == StageController.CurrentRoomData.Count)
                    Score.HitlessFinalBoss = true;
            }
            if (traversed)
            { 
                Score.TraverseBonus++;
                LogManager.Log("Left room through other exit. Increase traverse bonus");
            }
        }
        _tookDamage = false;
    }

    private static void CombatController_TookDamage()
    {
        _tookDamage = true;
        Score.CurrentKillStreak = 0;
        Score.TakenHits++;
    }

    #endregion

    private static void PassHistoryData(HistoryData entry, Enums.RunResult result)
    {
        entry.Score = Score.Copy();
        if (result == RunResult.Failed)
            entry.Score.Score = PDHelper.GeoPool;
        else if (result == RunResult.Forfeited)
            entry.Score.Score = PDHelper.GeoPool;
        entry.Score.EssenceBonus = PDHelper.DreamOrbs;
    }

    private static bool BlockPause(string name, bool orig)
    {
        if (name == nameof(PlayerData.disablePause))
            return orig || PhaseController.CurrentPhase == Phase.Result;
        return orig;
    }

    private static void CountGrubs(On.PlayerData.orig_IncrementInt orig, PlayerData self, string intName)
    {
        orig(self, intName);
        if (intName == nameof(PlayerData.grubsCollected))
        {
            PDHelper.GrubsCollected--;
            Score.GrubBonus++;
        }
    }

    #region Result show

    internal static void SetupScoreboard(GameObject prefab)
    {
        GameObject scoreObject = GameObject.Instantiate(prefab.LocateMyFSM("Challenge UI").GetState("Open UI").GetFirstAction<ShowBossDoorChallengeUI>().prefab.Value);
        scoreObject.name = "Scoreboard";
        UnityEngine.Object.Destroy(scoreObject.GetComponent<BossDoorChallengeUI>());
        GameObject buttonPrompt = scoreObject.transform.Find("Button Prompts").gameObject;
        GameObject confirmButton = buttonPrompt.transform.GetChild(1).gameObject;
        confirmButton.name = "Confirm Button";
        buttonPrompt.transform.GetChild(0).gameObject.SetActive(false);
        buttonPrompt.transform.GetChild(2).gameObject.SetActive(false);
        confirmButton.transform.Find("Text").GetComponent<TextMeshPro>().text = "Confirm";
        confirmButton.transform.position = new(-1.6f, -4.4f, -0.1f);
        int childCount = 0;
        foreach (Transform child in scoreObject.transform.Find("Panel"))
        {
            if (childCount > 1)
                UnityEngine.Object.Destroy(child.gameObject);
            else if (childCount == 1)
                child.name = "Score Text";
            childCount++;
        }
        ScoreboardPrefab = scoreObject;
        GameObject.DontDestroyOnLoad(ScoreboardPrefab);
    }

    internal static void SetupResultInspect(GameObject prefab)
    {
        ResultSequencePrefab = GameObject.Instantiate(prefab);
    }

    internal static void DisplayScore()
    {
        GameObject scoreboard = UnityEngine.Object.Instantiate(ScoreboardPrefab);
        scoreboard.transform.position = Vector3.zero;
        scoreboard.SetActive(true);
        scoreboard.GetComponent<Animator>().Play(0);

        TrialOfCrusaders.Holder.StartCoroutine(ScoreTally(scoreboard.transform.Find("Panel/Score Text").gameObject, 
            scoreboard.transform.Find("Button Prompts/Confirm Button").gameObject));
    }

    private static IEnumerator ScoreTally(GameObject textObject, GameObject confirmButton)
    {
        textObject.SetActive(false);
        confirmButton.SetActive(false);
        Score.Score = PDHelper.Geo;
        Score.EssenceBonus = PDHelper.DreamOrbs;
        List<(string, int)> values = Score.TransformToList();
        int position = 225;
        yield return new WaitForSeconds(1f);
        GameObject currentText = UnityEngine.Object.Instantiate(textObject, textObject.transform.parent);
        currentText.transform.localPosition = new(0f, 275f);
        currentText.GetComponent<Text>().text = "Final time: " + (Score.PassedTime >= 3600
            ? new DateTime(TimeSpan.FromSeconds(Score.PassedTime).Ticks).ToString("HH:mm:ss.ff")
            : new DateTime(TimeSpan.FromSeconds(Score.PassedTime).Ticks).ToString("mm:ss.ff"));
        currentText.GetComponent<Text>().fontStyle = FontStyle.Bold;
        currentText.GetComponent<Text>().fontSize++;
        currentText.SetActive(true);
        _finishedScores = 0;
        for (int i = 0; i < values.Count - 1; i++)
        {
            currentText = UnityEngine.Object.Instantiate(textObject, textObject.transform.parent);
            currentText.transform.localPosition = new(0f, position);
            position -= 50;
            if (i == 0)
                yield return FadeInText(currentText, values[i]);
            else
            {
                TrialOfCrusaders.Holder.StartCoroutine(FadeInText(currentText, values[i]));
                yield return new WaitForSeconds(1f);
            }
        }
        // For final score wait a bit
        while (_finishedScores != values.Count - 1)
            yield return null;
        currentText = UnityEngine.Object.Instantiate(textObject, textObject.transform.parent);
        currentText.transform.localPosition = new(0f, position);
        position -= 50;
        yield return FadeInText(currentText, values.Last());

        confirmButton.SetActive(true);
        bool pressed = false;
        float passedTime = 0f;
        while (!pressed)
        {
            passedTime += Time.deltaTime;
            if (passedTime >= 1f)
            {
                passedTime = 0f;
                confirmButton.SetActive(!confirmButton.activeSelf);
            }
            pressed = InputHandler.Instance.inputActions.jump.IsPressed;
            yield return null;
        }
        UnityEngine.Object.Destroy(textObject.transform.parent.parent.gameObject);
        PlayMakerFSM.BroadcastEvent("GG TRANSITION END");
    }

    private static IEnumerator FadeInText(GameObject textObject, (string, int) value)
    {
        textObject.SetActive(true);
        Text label = textObject.GetComponent<Text>();
        label.text = value.Item1;
        label.alignment = TextAnchor.MiddleLeft;
        GameObject point = UnityEngine.Object.Instantiate(textObject, textObject.transform.parent);
        Text pointValue = point.GetComponent<Text>();
        if (value.Item2 != 0)
            pointValue.text = "0";
        else
            pointValue.text = "-";
        pointValue.alignment = TextAnchor.MiddleRight;

        if (value.Item1 == ScoreData.FinalScoreField)
        {
            label.fontSize += 2;
            pointValue.fontSize += 2;
            label.fontStyle = FontStyle.Bold;
            pointValue.fontStyle = FontStyle.Bold;
        }
        else
        {
            label.fontSize++;
            pointValue.fontSize++;
        }
        float colorAlpha = 0f;
        while (colorAlpha < 1f)
        {
            colorAlpha += Time.deltaTime * 3;
            label.color = new(1f, 1f, 1f, colorAlpha);
            pointValue.color = new(1f, 1f, 1f, colorAlpha);
            yield return null;
        }

        if (value.Item2 == 0)
        {
            _finishedScores++;
            yield break;
        }
        int currentDisplayValue = 0;
        while (currentDisplayValue < value.Item2)
        {
            yield return null;
            currentDisplayValue += value.Item1 == ScoreData.FinalScoreField ? 20 : 10;
            pointValue.text = $"{currentDisplayValue}";
        }
        pointValue.text = $"{value.Item2}";
        _finishedScores++;
    }

    private static void SetupResultElements(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (self.gameObject.name == "Colosseum Manager")
        {
            GameCameras.instance.hudCanvas.LocateMyFSM("Slide Out").SendEvent("OUT");
            if (self.FsmName == "Manager")
            {
                // Skip unnecessary states.
                self.GetState("Init").AdjustTransitions("Idle");
                self.GetState("Init Cheer").RemoveFirstAction<ActivateGameObject>();
                self.GetState("Init Cheer").AdjustTransitions("Extra Title Pause");
                self.AddState("Wait", [new Wait() { time = 1f, finishEvent = self.FsmEvents.FirstOrDefault(x => x.Name == "FINISHED") }], FsmTransitionData.FromTargetState("Start Pause").WithEventName("FINISHED"));
                self.GetState("Extra Title Pause").AdjustTransitions("Wait");
                self.GetState("Start Pause").AddActions(() => self.gameObject.LocateMyFSM("Geo Pool").SendEvent("GIVE GEO"));

                // Prevent vanilla trial from starting.
                self.GetState("Waves Start").RemoveFirstAction<SendEventByName>();
            }
            else if (self.FsmName == "Geo Pool")
            {
                PDHelper.ColosseumSilverCompleted = true;
                // Each power decreases the final value. 2 are ignored as spells are forced by certain rooms.
                int baseGeoReward = HistoryController.TempEntry.Score.Mode == GameMode.GrandCrusader
                    ? 2500
                    : 1500;
                baseGeoReward = Math.Max(100, baseGeoReward - (HistoryController.TempEntry.Powers.Count - 2) * 50);
                if (HistoryController.TempEntry.Powers.Contains(typeof(VoidHeart).Name))
                    baseGeoReward *= 2;

                self.FsmVariables.FindFsmInt("Starting Pool").Value = baseGeoReward;
                self.AddState("Wait for Result", () =>
                {
                    PlayMakerFSM.BroadcastEvent("CROWD IDLE");
                    GameObject inspect = CreateInspect();
                    inspect.SetActive(true);
                    inspect.transform.position = new(102.41f, 10.8f);
                }, FsmTransitionData.FromTargetState("Open Gates").WithEventName("SHINY PICKED UP"));
                self.GetState("Achieve Check").AdjustTransitions("Wait for Result");
            }
        }
        orig(self);
    }

    private static void HeroController_FinishedEnteringScene(On.HeroController.orig_FinishedEnteringScene orig, HeroController self, bool setHazardMarker, bool preventRunBob)
    {
        orig(self, setHazardMarker, preventRunBob);
        if (GameManager.instance.sceneName.Contains("Bronze"))
        {
            StopTimer();
            // 102.41, 6.4
            GameObject pedestal = new("Pedestal");
            pedestal.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Other.Pedestal");
            pedestal.transform.position = new(102.41f, 6.2f, -0.1f);
            pedestal.AddComponent<BoxCollider2D>().size = new(2f, 2.4f);
            pedestal.transform.localScale = new(2f, 2f);
            pedestal.layer = 8; // Terrain layer
            pedestal.SetActive(true);
        }
    }

    private static GameObject CreateInspect()
    {
        GameObject inspect = GameObject.Instantiate(ResultSequencePrefab);
        inspect.name = "Result inspect";
        inspect.SetActive(true);
        inspect.transform.Find("Prompt Marker").localPosition = new(0, 1.7f);
        PlayMakerFSM fsm = inspect.LocateMyFSM("GG Boss UI");
        FsmState state = fsm.GetState("Open UI");
        state.RemoveAllActions();
        state.AdjustTransition("FINISHED", "Take Control");
        fsm.AddState("Return Control", () =>
        {
            HeroController.instance.RegainControl();
            HeroController.instance.StartAnimationControl();
            PlayMakerFSM.BroadcastEvent("SHINY PICKED UP");
            HistoryController.TempEntry.Score.Score = PDHelper.Geo;
            HistoryController.TempEntry.RunId = HistoryController.TempEntry.GetRunId();
            HistoryController.History ??= [];
            HistoryController.History.Add(HistoryController.TempEntry);
            if (HistoryController.History.Count > HistoryController.HistorySettings.HistoryAmount)
                HistoryController.History.RemoveAt(0);
            HistoryController.CheckArchiveUpdate();
            HistoryController.TempEntry = null;
            GameManager.instance.SaveGame();
            PhaseController.TransitionTo(Phase.Lobby);
            GameCameras.instance.hudCanvas.LocateMyFSM("Slide Out").SendEvent("IN");
            UnityEngine.Object.Destroy(inspect);
        });
        fsm.AddState("Display Score", () =>
        {
            PlayMakerFSM.BroadcastEvent("CROWD CHEER");
            StageController.PlayClearSound(false);
            DisplayScore();
            PlayMakerFSM.BroadcastEvent("CROWD IDLE");
        }, FsmTransitionData.FromTargetState("Return Control").WithEventName("GG TRANSITION END"));
        fsm.AddState("Score Pause", () => PlayMakerFSM.BroadcastEvent("CROWD STILL"), FsmTransitionData.FromTargetState("Display Score").WithEventName("FINISHED"));
        fsm.GetState("Score Pause").AddActions(new Wait() { time = 5f, finishEvent = fsm.FsmEvents.FirstOrDefault(x => x.Name == "FINISHED") });
        fsm.GetState("Challenge Audio").AdjustTransitions("Score Pause");

        fsm = inspect.LocateMyFSM("npc_control");
        fsm.FsmVariables.FindFsmFloat("Move To Offset").Value = 0f;

        fsm.GetState("In Range").AddActions(() =>
        {
            Transform promptObject = fsm.FsmVariables.FindFsmGameObject("Prompt").Value.transform.Find("Labels/Challenge");
            UnityEngine.Object.Destroy(promptObject.GetComponent<SetTextMeshProGameText>());
            promptObject.GetComponent<TextMeshPro>().text = "Finish";
        });
        return inspect;
    }

    #endregion

    #region Timer Handling

    public static void StartTimer()
    {
        if (_timer != null)
            TrialOfCrusaders.Holder.StopCoroutine(_timer);
        _timer = TrialOfCrusaders.Holder.StartCoroutine(Timer());
    }

    public static void StopTimer()
    {
        if (_timer != null)
            TrialOfCrusaders.Holder.StopCoroutine(_timer);
    }

    private static IEnumerator Timer()
    {
        while (true)
        {
            yield return null;
            Score.PassedTime += Time.deltaTime;
            if (GameManager.instance.IsGamePaused())
                yield return new WaitUntil(() => !GameManager.instance.IsGamePaused());
            // ToDo: Update timer object.
        }
    }

    #endregion
}
