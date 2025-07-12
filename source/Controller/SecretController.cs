using KorzUtils.Helper;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;
using TrialOfCrusaders.Powers.Rare;
using TrialOfCrusaders.Resources.Text;
using TrialOfCrusaders.SaveData;
using TrialOfCrusaders.UnityComponents.Other;
using UnityEngine;
using static TrialOfCrusaders.ControllerShorthands;

namespace TrialOfCrusaders.Controller;

public class SecretController : BaseController, ISaveData
{
    private readonly float[] _dummySequence = [ 0, 0, 0, 0, 0, 0, 270, 270, 270, 90, 90, 90, 90, 90, 90, 90, 90, 90, 180, 180, 180];

    private readonly Dictionary<int, string> _stageHints = new()
    {
        {4, "Skip" },
        {13, "One" },
        {25, "Of" },
        {41, "Combat" },
        {57, "Spirit" },
        {70, "Endurance" },
        {82, "For" },
        {99, "Fortune" },
    };

    #region Properties

    public bool UnlockedToughness { get; set; }

    public bool UnlockedHighRoller { get; set; }

    public bool UnlockedStashedContraband { get; set; }

    public bool UnlockedSecretArchive { get; set; }

    public int ShopLevel { get; set; } = 4;

    public int LeftRolls { get; set; } = 3;

    public bool[] SkippedOrbs { get; set; } = [false, false, false];

    public List<float> DummyHitSequence { get; set; } = [];

    #endregion

    public SecretController()
    {
        PhaseManager.PhaseChanged += CheckForReset;
    }

    private void CheckForReset(Phase currentPhase, Phase newPhase)
    {
        if (newPhase == Phase.Run)
            SkippedOrbs = [false, false, false];
    }

    public override Phase[] GetActivePhases() => [Phase.Run, Phase.Result, Phase.Lobby];

    protected override void Enable()
    {
        LogManager.Log("Enabled secret controller");
        SkippedOrbs = [false, false, false];
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        LeftShinyFlag.LeftShinyBehind += LeftShinyFlag_LeftShinyBehind;
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
    }

    protected override void Disable()
    {
        LogManager.Log("Disabled secret controller");
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        LeftShinyFlag.LeftShinyBehind -= LeftShinyFlag_LeftShinyBehind;
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
    }

    internal void SetupItemScreen(PlayMakerFSM fsm, TreasureType treasure)
    {
        string itemName, description;
        switch (treasure)
        {
            case TreasureType.StashedContraband:
                itemName = SecretText.StashedContrabandTitle;
                description = SecretText.StashedContrabandDesc;
                UnlockedStashedContraband = true;
                break;
            case TreasureType.Toughness:
                itemName = SecretText.ToughnessTitle;
                description = SecretText.ToughnessDesc;
                UnlockedToughness = true;
                break;
            case TreasureType.Highroller:
                itemName = SecretText.HighrollerTitle;
                description = SecretText.HighrollerDesc;
                UnlockedHighRoller = true;
                LeftRolls = 3;
                break;
            case TreasureType.Archive:
                itemName = SecretText.ArchiveTitle;
                description = SecretText.ArchiveDesc;
                UnlockedSecretArchive = true;
                break;
            default:
                itemName = "Unknown";
                description = "Unknown";
                break;
        }

        // Same template
        fsm.FsmVariables.FindFsmGameObject("Item Name Prefix").Value.GetComponent<TextMeshPro>().text = "You unlocked:";
        fsm.FsmVariables.FindFsmGameObject("Press").Value.GetComponent<TextMeshPro>().text = "";
        fsm.FsmVariables.FindFsmGameObject("Button").Value.SetActive(false);
        fsm.FsmVariables.FindFsmGameObject("Msg 2").Value.GetComponent<TextMeshPro>().text = "This is a permanent upgrade.";

        fsm.FsmVariables.FindFsmGameObject("Item Name").Value.GetComponent<TextMeshPro>().text = itemName;
        fsm.FsmVariables.FindFsmGameObject("Msg 1").Value.GetComponent<TextMeshPro>().text = description;
        fsm.transform.Find("Icon").GetComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>($"Sprites.Icons.{treasure}_Icon");
    }

    internal string CheckForStageHints()
    {
        if (_stageHints.ContainsKey(StageRef.CurrentRoomNumber))
            return $" ({_stageHints[StageRef.CurrentRoomNumber]})";
        return "";
    }

    public void ReceiveSaveData(LocalSaveData saveData)
    {
        UnlockedHighRoller = saveData.UnlockedHighRoller;
        UnlockedSecretArchive = saveData.UnlockedSecretArchive;
        UnlockedStashedContraband = saveData.UnlockedContraband;
        UnlockedToughness = saveData.UnlockedToughness;
    }

    public void UpdateSaveData(LocalSaveData saveData)
    {
        saveData.UnlockedHighRoller = UnlockedHighRoller;
        saveData.UnlockedSecretArchive = UnlockedSecretArchive;
        saveData.UnlockedContraband = UnlockedStashedContraband;
        saveData.UnlockedToughness = UnlockedToughness;
    }

    #region Eventhandler

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        if (arg1.name == "Room_Colosseum_Bronze")
        { 
            if (!UnlockedStashedContraband && RngManager.Seed == 777777777)
                TreasureManager.SpawnShiny(Enums.TreasureType.StashedContraband, new(60.1f, 6.4f), false);
            if (!UnlockedToughness && SkippedOrbs.All(x => x))
                TreasureManager.SpawnShiny(Enums.TreasureType.Toughness, new(55.1f, 6.4f), false);
        }
        else if (arg1.name == "Dream_Room_Believer_Shrine")
        {
            if (!UnlockedSecretArchive && HistoryRef.History.Count > 0 && HistoryRef.History.Last().Result == RunResult.Completed
                && HistoryRef.History.Last().Powers.Contains(TreasureManager.GetPower<VoidHeart>().Name))
                TreasureManager.SpawnShiny(TreasureType.Archive, new(26.15f, 47.4f), false);
        }
    }

    private void LeftShinyFlag_LeftShinyBehind(TreasureType treasure)
    {
        if (treasure == TreasureType.CombatOrb)
            SkippedOrbs[0] = true;
        else if (treasure == TreasureType.SpiritOrb)
            SkippedOrbs[1] = true;
        else if (treasure == TreasureType.EnduranceOrb)
            SkippedOrbs[2] = true;
    }

    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (self.FsmName == "Hit" && self.gameObject.name == "Training Dummy")
        {
            DummyHitSequence.Clear();
            self.GetState("Summon?").RemoveAllActions();
            self.GetState("Recover").AddActions(() =>
            {
                if (!UnlockedHighRoller && !DummyHitSequence.Contains(-1))
                {
                    DummyHitSequence.Add(self.FsmVariables.FindFsmFloat("Attack Direction").Value);
                    LogManager.Log(DummyHitSequence.Last().ToString());
                    if (DummyHitSequence.Count > _dummySequence.Length)
                        DummyHitSequence.RemoveAt(0);
                    if (DummyHitSequence.SequenceEqual(_dummySequence))
                    {
                        DummyHitSequence.Clear();
                        // Flag to prevent multiple spawns.
                        DummyHitSequence.Add(-1f);
                        TreasureManager.SpawnShiny(TreasureType.Highroller, new(35.38f, 17.3f), false);
                    }
                }
            });
        }
        orig(self);
    }

    #endregion
}
