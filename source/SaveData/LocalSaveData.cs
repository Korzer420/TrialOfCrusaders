using System.Collections.Generic;
using TrialOfCrusaders.Data;

namespace TrialOfCrusaders.SaveData;

public class LocalSaveData
{
    #region Non-Run Data

    public List<HistoryData> OldRunData { get; set; } = [];

    public ArchiveData Archive { get; set; } = new();

    #endregion

    // ToDo: Support save inside a run.
    //#region Stage Data

    //public int CurrentRoomNumber { get; set; }

    //public List<string> RoomList { get; set; } = [];

    //#endregion

    //#region Power flags

    //public bool FragileGreedActive { get; set; }

    //public bool FragileSpiritActive { get; set; }

    //public bool FragileStrengthActive { get; set; }

    //public bool CocoonUsed { get; set; }

    //public bool FocusEnergyUsed { get; set; }

    //public bool InUtterDarknessUsed { get; set; }

    //public bool DamoclesUsed { get; set; }

    //#endregion

    //#region Combat Data

    //public List<string> ObtainedPowers { get; set; } = [];

    //public int CombatLevel { get; set; }

    //public int SpiritLevel { get; set; }

    //public int EnduranceLevel { get; set; }

    //public int CurrentHealth { get; set; }

    //#endregion

    //#region Other

    //public ScoreData Score { get; set; } = new();

    //public int RandomSeed { get; set; }

    //public Progress CurrentProgress { get; set; }

    //#endregion

    //#region Save Data Crawler

    //public LocalSaveData GetLobbyData() => new() { OldRunData = OldRunData };

    //public LocalSaveData GetFixedData() => new()
    //{
    //    OldRunData = OldRunData,
    //    CurrentRoomNumber = CurrentRoomNumber,
    //    RoomList = RoomList,
    //    ObtainedPowers = ObtainedPowers,
    //    CombatLevel = CombatLevel,
    //    SpiritLevel = SpiritLevel,
    //    EnduranceLevel = EnduranceLevel,
    //    CurrentHealth = CurrentHealth,
    //    RandomSeed = RngProvider.Seed,
    //    CurrentProgress = CurrentProgress,
    //    Score = Score.Copy(),
    //    // All these values are updated even though the rest isn't.
    //    // This is to prevent cheese by resetting the game upon loosing one of these one time effects.
    //    CocoonUsed = CombatController.HasPower<Cocoon>(out _),
    //    DamoclesUsed = CombatController.HasPower(out Damocles damocles) && damocles.Triggered,
    //    FocusEnergyUsed = CombatController.HasPower<FocusedEnergy>(out _),
    //    InUtterDarknessUsed = CombatController.HasPower<InUtterDarkness>(out _),
    //    FragileGreedActive = !CombatController.HasPower(out FragileGreed greed) || greed.GreedActive,
    //    FragileSpiritActive = !CombatController.HasPower(out FragileSpirit spirit) || spirit.SpiritActive,
    //    FragileStrengthActive = !CombatController.HasPower(out FragileStrength strength) || strength.StrengthActive
    //};

    //public LocalSaveData GetUpdatedData() => new()
    //{
    //    OldRunData = OldRunData,
    //    CurrentRoomNumber = StageController.CurrentRoomNumber,
    //    RoomList = [.. StageController.CurrentRoomData.Select(x => $"{x.Name}{x.SelectedTransition}")],
    //    ObtainedPowers = [.. CombatController.ObtainedPowers.Select(x => x.Name)],
    //    CombatLevel = CombatController.CombatLevel,
    //    SpiritLevel = CombatController.SpiritLevel,
    //    EnduranceLevel = CombatController.EnduranceLevel,
    //    CurrentHealth = PDHelper.Health,
    //    RandomSeed = RngProvider.Seed,
    //    CurrentProgress = DetermineProgress(),
    //    Score = ScoreController.Score.Copy(),
    //    CocoonUsed = CombatController.HasPower(out Cocoon cocoon),
    //    DamoclesUsed = CombatController.HasPower(out Damocles damocles) && damocles.Triggered,
    //    FocusEnergyUsed = CombatController.HasPower<FocusedEnergy>(out _),
    //    InUtterDarknessUsed = CombatController.HasPower<InUtterDarkness>(out _),
    //    FragileGreedActive = !CombatController.HasPower(out FragileGreed greed) || greed.GreedActive,
    //    FragileSpiritActive = !CombatController.HasPower(out FragileSpirit spirit) || spirit.SpiritActive,
    //    FragileStrengthActive = !CombatController.HasPower(out FragileStrength strength) || strength.StrengthActive
    //};

    //#endregion

    //private Progress DetermineProgress()
    //{
    //    Progress currentProgress = Progress.None;
    //    if (PDHelper.HasDash)
    //        currentProgress |= Progress.Dash;
    //    if (PDHelper.HasWalljump)
    //        currentProgress |= Progress.Claw;
    //    if (PDHelper.HasDoubleJump)
    //        currentProgress |= Progress.Wings;
    //    if (PDHelper.HasShadowDash)
    //        currentProgress |= Progress.ShadeCloak;
    //    if (PDHelper.HasLantern)
    //        currentProgress |= Progress.Lantern;
    //    if (PDHelper.HasSuperDash)
    //        currentProgress |= Progress.CrystalHeart;
    //    if (PDHelper.HasAcidArmour)
    //        currentProgress |= Progress.Tear;
    //    return currentProgress;
    //}
}
