using System.Collections.Generic;
using TrialOfCrusaders.Data;

namespace TrialOfCrusaders.SaveData;

public class LocalSaveData
{
    #region Non-Run Data

    public List<HistoryData> OldRunData { get; set; } = [];

    public ArchiveData Archive { get; set; } = new();

    public bool UnlockedSecretArchive { get; set; }

    public bool UnlockedHighRoller { get; set; }

    public bool UnlockedContraband { get; set; }

    public bool UnlockedToughness { get; set; }

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
    //    CocoonUsed = CombatRef.HasPower<Cocoon>(out _),
    //    DamoclesUsed = CombatRef.HasPower(out Damocles damocles) && damocles.Triggered,
    //    FocusEnergyUsed = CombatRef.HasPower<FocusedEnergy>(out _),
    //    InUtterDarknessUsed = CombatRef.HasPower<InUtterDarkness>(out _),
    //    FragileGreedActive = !CombatRef.HasPower(out FragileGreed greed) || greed.GreedActive,
    //    FragileSpiritActive = !CombatRef.HasPower(out FragileSpirit spirit) || spirit.SpiritActive,
    //    FragileStrengthActive = !CombatRef.HasPower(out FragileStrength strength) || strength.StrengthActive
    //};

    //public LocalSaveData GetUpdatedData() => new()
    //{
    //    OldRunData = OldRunData,
    //    CurrentRoomNumber = StageRef.CurrentRoomNumber,
    //    RoomList = [.. StageRef.CurrentRoomData.Select(x => $"{x.Name}{x.SelectedTransition}")],
    //    ObtainedPowers = [.. CombatRef.ObtainedPowers.Select(x => x.Name)],
    //    CombatLevel = CombatRef.CombatLevel,
    //    SpiritLevel = CombatRef.SpiritLevel,
    //    EnduranceLevel = CombatRef.EnduranceLevel,
    //    CurrentHealth = PDHelper.Health,
    //    RandomSeed = RngProvider.Seed,
    //    CurrentProgress = DetermineProgress(),
    //    Score = ScoreRef.Score.Copy(),
    //    CocoonUsed = CombatRef.HasPower(out Cocoon cocoon),
    //    DamoclesUsed = CombatRef.HasPower(out Damocles damocles) && damocles.Triggered,
    //    FocusEnergyUsed = CombatRef.HasPower<FocusedEnergy>(out _),
    //    InUtterDarknessUsed = CombatRef.HasPower<InUtterDarkness>(out _),
    //    FragileGreedActive = !CombatRef.HasPower(out FragileGreed greed) || greed.GreedActive,
    //    FragileSpiritActive = !CombatRef.HasPower(out FragileSpirit spirit) || spirit.SpiritActive,
    //    FragileStrengthActive = !CombatRef.HasPower(out FragileStrength strength) || strength.StrengthActive
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
