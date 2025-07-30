using System;
using System.Collections.Generic;
using System.Linq;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;

namespace TrialOfCrusaders.Controller.GameController;

public class GrandCrusaderController : GameModeController
{
    public override GameMode Mode => GameMode.GrandCrusader;

    public override string Explanation => Resources.Text.LobbyDialog.ExplanationTrialGrandCrusader;

    public override List<RoomData> GenerateRoomList(bool atStart)
    {
        List<RoomData> roomList = [];
        try
        {
            List<RoomData> availableRooms = StageController.LoadRoomData();
            Progress currentProgress = Progress.None;
            Dictionary<int, Progress> progressItemRooms = RollAbilityRooms();
            // Used to prevent room repeats under 15 rooms.
            List<string> lastRooms = [];
            int lastBossRoom = -1;
            for (int currentRoom = 0; currentRoom < 100; currentRoom++)
            {
                // Ability rooms/Treasure rooms are not part of the normal routine.
                // The latter will be generated on the spot, but since the abilities open more rooms, we calculate them before hand.
                if (progressItemRooms.ContainsKey(currentRoom))
                {
                    roomList.Add(new() { Name = progressItemRooms[currentRoom].ToString(), SelectedTransition = "Warp" });
                    currentProgress |= progressItemRooms[currentRoom];
                    continue;
                }
                List<RoomData> reachableRooms = [];
                foreach (RoomData room in availableRooms)
                    if (room.Available(false, currentProgress))
                    {
                        if (!room.BossRoom && (lastRooms.Contains(room.Name) || (currentRoom > 0 && currentRoom % 20 == 0)))
                            continue;
                        else if (room.BossRoom && (currentRoom < 30 || (currentRoom <= lastBossRoom + 2 && currentRoom % 20 != 0)))
                            continue;
                        reachableRooms.Add(room);
                    }
                RoomData rolledRoom = reachableRooms[RngManager.GetRandom(0, reachableRooms.Count - 1)];
                availableRooms.Remove(rolledRoom);
                // Boss rooms and big rooms can only appear once, but they are in the list to increase the chance.
                if (rolledRoom.BossRoom || rolledRoom.BigRoom)
                    availableRooms.RemoveAll(x => x.Name == rolledRoom.Name);
                lastRooms.Add(rolledRoom.Name);
                roomList.Add(rolledRoom);
                if (rolledRoom.BossRoom)
                    lastBossRoom = roomList.Count;
                if (lastRooms.Count == 16)
                    lastRooms.RemoveAt(0);
            }
            // This should only leave NKG, Pure Vessel and Radiance which we use as end bosses.
            availableRooms = [.. availableRooms.Where(x => x.BossRoom && !x.Available(false, currentProgress))];
            roomList.Add(availableRooms[RngManager.GetRandom(0, availableRooms.Count - 1)]);

            // Test specific boss.
            //var roomData = StageRef.LoadRoomData().First(x => x.Name == "GG_Dung_Defender");
            //roomList.Insert(91, roomData);
            // Test specific room.
            //var selectedRoomData = StageRef.LoadRoomData().First(x => x.Name == "Fungus2_14");
            // Test room at start.
            //roomList = [..selectedRoomData.AllowedEntrances.Select(x => new RoomData()
            //{
            //    Name = selectedRoomData.Name,
            //    SelectedTransition = x
            //}), .. roomList];
            // Test room insert
            //roomList.Insert(30, new RoomData()
            //{
            //    SelectedTransition = "right1",
            //    Name = selectedRoomData.Name
            //});
        }
        catch (Exception exception)
        {
            LogManager.Log("Error in setup run.", exception);
        }
        return roomList;
    }

    private Dictionary<int, Progress> RollAbilityRooms()
    {
        bool fireballFirst = RngManager.GetRandom(0, 1) == 0;
        List<int> availableAbilityRooms = [.. Enumerable.Range(3, 90)];
        // 40 and 80 are reserved for fireball + quake (exclude the rooms before and after as well)
        for (int i = 37; i < 82; i++)
        {
            availableAbilityRooms.Remove(i);
            if (i == 41)
                i = 77;
        }
        int dashRoom, clawRoom, wingRoom, cDashRoom, tearRoom, shadeCloakRoom, lanternRoom;
        dashRoom = RngManager.GetRandom(2, 30);
        availableAbilityRooms.RemoveAll(x => x >= dashRoom - 3 && x <= dashRoom + 3);
        List<int> clawSelection = [.. availableAbilityRooms.Where(x => x <= 35 && x >= 20)];
        clawRoom = clawSelection[RngManager.GetRandom(0, clawSelection.Count - 1)];
        availableAbilityRooms.RemoveAll(x => x >= clawRoom - 3 && x <= clawRoom + 3);
        wingRoom = availableAbilityRooms[RngManager.GetRandom(0, availableAbilityRooms.Count - 1)];
        availableAbilityRooms.RemoveAll(x => x >= wingRoom - 3 && x <= wingRoom + 3);
        cDashRoom = availableAbilityRooms[RngManager.GetRandom(0, availableAbilityRooms.Count - 1)];
        availableAbilityRooms.RemoveAll(x => x >= cDashRoom - 3 && x <= cDashRoom + 3);
        tearRoom = availableAbilityRooms[RngManager.GetRandom(0, availableAbilityRooms.Count - 1)];
        availableAbilityRooms.RemoveAll(x => x >= tearRoom - 3 && x <= tearRoom + 3);
        lanternRoom = availableAbilityRooms[RngManager.GetRandom(0, availableAbilityRooms.Count - 1)];
        availableAbilityRooms.RemoveAll(x => x >= lanternRoom - 3 && x <= lanternRoom + 3);
        // Shade cloak is only available after dash
        availableAbilityRooms.RemoveAll(x => x < dashRoom + 3);
        shadeCloakRoom = availableAbilityRooms[RngManager.GetRandom(0, availableAbilityRooms.Count - 1)];
        return new()
        {
            { dashRoom, Progress.Dash },
            { clawRoom, Progress.Claw },
            { wingRoom, Progress.Wings },
            { cDashRoom, Progress.CrystalHeart },
            { tearRoom, Progress.Tear },
            { shadeCloakRoom, Progress.ShadeCloak },
            { lanternRoom, Progress.Lantern },
            { 39, fireballFirst ? Progress.Fireball : Progress.Quake },
            { 79, fireballFirst ? Progress.Quake : Progress.Fireball }
        };
    }

    public override bool CheckForEnding() => ControllerShorthands.StageRef.CurrentRoomData.Count == ControllerShorthands.StageRef.CurrentRoomIndex;
}
