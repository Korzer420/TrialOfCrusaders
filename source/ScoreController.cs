using KorzUtils.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TrialOfCrusaders;

public static class ScoreController
{
    private static int _currentHitlessRoomStreak;
    private static int _currentKillStreak;
    private static Coroutine _timer;
    private static int _finishedScores = 0;

    #region Properties

    public static GameObject Prefab { get; set; }

    public static int CurrentHitlessRoomStreak
    {
        get => _currentHitlessRoomStreak;
        set
        {
            _currentHitlessRoomStreak = value;
            if (value > HighestHitlessRoomStreak)
                HighestHitlessRoomStreak = value;
        }
    }

    public static int CurrentKillStreak
    {
        get => _currentKillStreak;
        set
        {
            _currentKillStreak = value;
            if (value > _currentKillStreak)
                HighestKillStreak = value;
        }
    }

    public static int HitlessBosses { get; set; }

    public static bool HitlessFinalBoss { get; set; }

    public static int TotalHitlessRooms { get; set; }

    public static int HighestHitlessRoomStreak { get; set; }

    public static int HighestKillStreak { get; set; }

    public static float PassedTime { get; set; }

    #endregion

    public static void DisplayScore()
    {
        GameObject scoreboard = GameObject.Instantiate(Prefab);
        scoreboard.name = "Scoreboard";
        Component.Destroy(scoreboard.GetComponent<BossDoorChallengeUI>());
        GameObject buttonPrompt = scoreboard.transform.Find("Button Prompts").gameObject;
        GameObject confirmButton = buttonPrompt.transform.GetChild(1).gameObject;
        buttonPrompt.transform.GetChild(0).gameObject.SetActive(false);
        buttonPrompt.transform.GetChild(2).gameObject.SetActive(false);
        GameObject textObject = null;
        int childCount = 0;
        foreach (Transform child in scoreboard.transform.Find("Panel"))
        {
            if (childCount > 1)
                GameObject.Destroy(child.gameObject);
            else if (childCount == 1)
            { 
                child.name = "TextObject";
                textObject = child.gameObject;
            }
            childCount++;
        }
        scoreboard.transform.position = Vector3.zero;
        scoreboard.SetActive(true);
        scoreboard.GetComponent<Animator>().Play(0);
        confirmButton.transform.Find("Text").GetComponent<TextMeshPro>().text = "Confirm";
        confirmButton.transform.position = new(-1.6f, -4.4f, -0.1f);
        TrialOfCrusaders.Holder.StartCoroutine(ScoreTally(textObject, confirmButton));
    }

    private static IEnumerator ScoreTally(GameObject textObject, GameObject confirmButton)
    {
        textObject.SetActive(false);
        confirmButton.SetActive(false);
#if DEBUG
        PDHelper.Geo = 3500;
        PDHelper.DreamOrbs = 120;
        PassedTime = 1400f;
        HighestKillStreak = 65;
        TotalHitlessRooms = 34;
        HighestHitlessRoomStreak = 20;
        HitlessBosses = 4;
        HitlessFinalBoss = true;
#endif
        /*
         Score:
        - 1 per geo.
        - +200 per Hitless boss (except the last)
        - +1000 if last boss was hitless
        - 10 per Dream Essence
        - +20 per Hitless room
        - Double hitless points for highest hitless room streak.
        - 5 Points per enemy in the longest Enemy kill streak (without taking damage).
        - 1 Point per second before 1 hour.
         */
        List<(string, int)> values =
        [
            new("Score:", PDHelper.Geo),
            new("Essence bonus:", PDHelper.DreamOrbs * 10),
            new("Time bonus:", Math.Max(0, 3600 - Mathf.CeilToInt(PassedTime))),
            new("Killstreak bonus:", HighestKillStreak * 5),
            new("Flawless stage bonus:", TotalHitlessRooms * 20),
            new("Perfect streak bonus:", HighestHitlessRoomStreak * 20),
            new("Perfect boss bonus:", HitlessBosses * 200),
            new("Perfect final bonus:", HitlessFinalBoss ? 1000 : 0),
        ];
        values.Add(new("Final score:", values.Select(x => x.Item2).Sum()));
        int position = 225;
        yield return new WaitForSeconds(1f);
        GameObject currentText = GameObject.Instantiate(textObject, textObject.transform.parent);
        currentText.transform.localPosition = new(0f, 275f);
        currentText.GetComponent<Text>().text = "Final time: " + (PassedTime >= 3600
            ? new DateTime(TimeSpan.FromSeconds(PassedTime).Ticks).ToString("HH:mm:ss.ff")
            : new DateTime(TimeSpan.FromSeconds(PassedTime).Ticks).ToString("mm:ss.ff"));
        currentText.GetComponent<Text>().fontStyle = FontStyle.Bold;
        currentText.GetComponent<Text>().fontSize++;
        currentText.SetActive(true);
        _finishedScores = 0;
        for (int i = 0; i < 8; i++)
        {
            currentText = GameObject.Instantiate(textObject, textObject.transform.parent);
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
        yield return new WaitUntil(() => _finishedScores == 8);
        currentText = GameObject.Instantiate(textObject, textObject.transform.parent);
        currentText.transform.localPosition = new(0f, position);
        position -= 50;
        yield return FadeInText(currentText, values[8]);

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
        GameObject.Destroy(textObject.transform.parent.parent.gameObject);
        PlayMakerFSM.BroadcastEvent("GG TRANSITION END");
    }

    private static IEnumerator FadeInText(GameObject textObject, (string,int) value)
    {
        textObject.SetActive(true);
        Text label = textObject.GetComponent<Text>();
        label.text = value.Item1;
        label.alignment = TextAnchor.MiddleLeft;
        GameObject point = GameObject.Instantiate(textObject, textObject.transform.parent);
        Text pointValue = point.GetComponent<Text>();
        if (value.Item2 != 0)
            pointValue.text = "0";
        else
            pointValue.text = "-";
        pointValue.alignment = TextAnchor.MiddleRight;

        if (value.Item1 == "Final score:")
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
        while(colorAlpha < 1f)
        {
            colorAlpha += Time.deltaTime;
            label.color = new(1f,1f,1f, colorAlpha);
            pointValue.color = new(1f, 1f, 1f, colorAlpha);
            yield return null;
        }

        if (value.Item2 == 0)
            yield break;
        int currentDisplayValue = 0;
        while (currentDisplayValue < value.Item2)
        {
            yield return null;
            currentDisplayValue += value.Item1 == "Final score:" ? 10 : 5;
            pointValue.text = $"{currentDisplayValue}";
        }
        pointValue.text = $"{value.Item2}";
        _finishedScores++;
    }

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
            PassedTime += Time.deltaTime;
            if (GameManager.instance.IsGamePaused())
                yield return new WaitUntil(() => !GameManager.instance.IsGamePaused());
            // ToDo: Update timer object.
        }
    } 

    #endregion
}
