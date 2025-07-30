using KorzUtils.Helper;
using System.Collections.Generic;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;

namespace TrialOfCrusaders.Controller.GameController;

internal class GoldRushController : GameModeController
{
    public override GameMode Mode => GameMode.GoldRush;

    public override string Explanation => "Have 5000 geo in your pocket and exit the current room to win. You may spend geo in the shop, but that might delay your freedom. Enemies will become stronger the more rooms you progress.";

    public override List<RoomData> GenerateRoomList(bool atStart)
    {
        Progress progress = Progress.None;
        if (!atStart)
        {
            if (PDHelper.HasDash || PDHelper.CanDash)
                progress |= Progress.Dash;
            if (PDHelper.HasWalljump || PDHelper.CanWallJump)
                progress |= Progress.Claw;
            if (PDHelper.HasSuperDash)
                progress |= Progress.CrystalHeart;
            if (PDHelper.HasLantern)
                progress |= Progress.Lantern;
            if (PDHelper.HasAcidArmour)
                progress |= Progress.Tear;
            if (PDHelper.HasDoubleJump)
                progress |= Progress.Wings;
            if (PDHelper.HasShadowDash)
                progress |= Progress.ShadeCloak;
            if (PDHelper.FireballLevel > 0)
                progress |= Progress.Fireball;
            if (PDHelper.QuakeLevel > 0)
                progress |= Progress.Quake;
        }
        int currentRoomIndex = ControllerShorthands.StageRef.CurrentRoomIndex;
        List<RoomData> rooms = StageController.LoadRoomData();
        List<RoomData> availableRooms = [];
        List<string> addedRooms = [];
        foreach (RoomData room in rooms)
            if (room.Available(false, progress))
            {
                if (room.BossRoom && currentRoomIndex < 20)
                    continue;
                else if (!room.BossRoom && currentRoomIndex % 20 == 0 && currentRoomIndex > 0)
                    continue;
                if (!addedRooms.Contains(room.Name))
                {
                    availableRooms.Add(room);
                    addedRooms.Add(room.Name);
                }
            }
        List<RoomData> selectedRooms = [];
        for (int i = 0; i < 20; i++)
        {
            selectedRooms.Add(availableRooms[RngManager.GetRandom(0, availableRooms.Count - 1)]);
            availableRooms.Remove(selectedRooms[selectedRooms.Count - 1]);
        }

        // Check if all items have been obtained.
        if ((int)progress != 511)
        {
            List<Progress> values = [..(System.Enum.GetValues(typeof(Progress)) as Progress[])];
            Progress selectedProgressItem = values[0];
            while (progress.HasFlag(selectedProgressItem))
            { 
                values.Remove(selectedProgressItem);
                selectedProgressItem = values[RngManager.GetRandom(0, values.Count - 1)]; 
            }
            selectedRooms.Insert(8, new()
            {
                Name = selectedProgressItem.ToString(),
                SelectedTransition = "Warp"
            });
        }
        return selectedRooms;
    }

    public override bool CheckForEnding()
    {
        if (PDHelper.Geo >= 5000)
            return true;
        if (ControllerShorthands.StageRef.CurrentRoomData.Count == ControllerShorthands.StageRef.CurrentRoomIndex)
            ControllerShorthands.StageRef.CurrentRoomData.AddRange(GenerateRoomList(false));
        return false;
    }

}
