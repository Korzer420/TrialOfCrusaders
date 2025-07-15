using KorzUtils.Helper;
using System.Collections;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Rare;

internal class Banish : Power
{
    private Coroutine _listener;

    #region Power Data
    
    public override Rarity Tier => Rarity.Rare;

    public override bool CanAppear => !HasPower<DreamPortal>();

    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override DraftPool Pools => DraftPool.Retain | DraftPool.Ability;

    public override string Description
    {
        get
        {
            string text = Resources.Text.PowerDescriptions.Banish;
            if (BanishedScene != null)
                text += " (Currently Banished: " + BanishedScene + ")";
            return text;
        }
    }

    public string BanishedScene { get; set; }

    #endregion

    public bool CanInput => HeroController.instance?.acceptingInput == true && GameManager.instance?.IsGamePaused() == false;

    protected override void Enable()
    {
        if (_listener != null)
            StopRoutine(_listener);
        _listener = StartRoutine(WaitForInput());
    }

    protected override void Disable()
    {
        if (_listener != null)
            StopRoutine(_listener);
    }

    private IEnumerator WaitForInput()
    {
        float holdTime = 0f;
        while (true)
        {
            if (StageRef.QuietRoom || GameManager.instance.IsGamePaused())
                yield return new WaitUntil(() => !StageRef.QuietRoom && !GameManager.instance.IsGamePaused());
            yield return null;
            if (InputHandler.Instance.inputActions.quickMap.IsPressed)
            {
                holdTime += Time.deltaTime;

                if (holdTime >= 5)
                {
                    BanishedScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                    if (BanishedScene == "GG_Radiance")
                    {
                        BanishedScene = null;
                        GameHelper.DisplayMessage("Your banish didn't work...");
                    }
                    else if (BanishedScene == "GG_Hollow_Knight")
                    {
                        BanishedScene = null;
                        GameHelper.DisplayMessage("Somehow... your cancelled the banish...");
                    }
                    else
                        GameHelper.DisplayMessage("This room will no longer appear in future trials.");
                    yield break;
                }
            }
            else
                holdTime = 0f;
        }
    }
}
