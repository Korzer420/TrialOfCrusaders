using System;
using System.Collections.Generic;
using System.Linq;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Manager;

/// <summary>
/// Provides methods to help setup a run.
/// </summary>
internal static class SetupManager
{
    #region Mode Setups

    internal static List<RoomData> GenerateNormalRun()
    {
        List<RoomData> roomList = [];
        try
        {
            Progress currentProgress = Progress.None;
            // Add each normal room 5 times so each one has the same probability regardless of available entrances.
            // For bosses we only take two (although they only can appear once).
            List<RoomData> availableRooms = [..StageController.LoadRoomData().SelectMany(x =>
        {
            if (x.BossRoom)
                return [x, x];
            List<RoomData> roomCopies = [..x.AllowedEntrances.Select(y => new RoomData()
            {
                Name = x.Name,
                ConditionalProgress = x.ConditionalProgress,
                NeededProgress = x.NeededProgress,
                EasyConditionalProgress = x.EasyConditionalProgress,
                EasyNeededProgress = x.EasyNeededProgress,
                SelectedTransition = y
            })];
            if (roomCopies.Count < 5)
                for (int i = roomCopies.Count; i <= 5; i++)
                    roomCopies.Add(new RoomData()
                    {
                        Name = x.Name,
                        ConditionalProgress = x.ConditionalProgress,
                        NeededProgress = x.NeededProgress,
                        EasyConditionalProgress = x.EasyConditionalProgress,
                        EasyNeededProgress = x.EasyNeededProgress,
                        SelectedTransition = x.AllowedEntrances[RngManager.GetRandom(0, x.AllowedEntrances.Count - 1)]
                    });
            return roomCopies;
        })];
            Dictionary<int, Progress> progressItemRooms = RollAbilityRooms();
            // Used to prevent room repeats under 15 rooms.
            List<string> lastRooms = [];
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
                    if (room.Available(false, currentProgress, currentRoom))
                    {
                        // For none boss rooms ensure the same room cannot occur between 15 rooms of each other.
                        // For boss rooms ensure that no boss appear
                        if (!room.BossRoom && lastRooms.Contains(room.Name)
                            || room.BossRoom && (roomList[roomList.Count - 1].BossRoom || roomList[roomList.Count - 2].BossRoom) && currentRoom % 20 != 0)
                            continue;
                        reachableRooms.Add(room);
                    }
                RoomData rolledRoom = reachableRooms[RngManager.GetRandom(0, reachableRooms.Count - 1)];
                availableRooms.Remove(rolledRoom);
                // Boss rooms are in the list twice to increase the chance of appearing, but we are only allow one of each.
                // Therefore, we remove the duplicate as well.
                if (rolledRoom.BossRoom)
                    availableRooms.RemoveAll(x => x.Name == rolledRoom.Name);
                lastRooms.Add(rolledRoom.Name);
                roomList.Add(rolledRoom);
                if (lastRooms.Count == 16)
                    lastRooms.RemoveAt(0);
            }
            // This should only leave NKG, Pure Vessel and Radiance which we use as end bosses.
            availableRooms = [.. availableRooms.Where(x => x.BossRoom && !x.Available(false, currentProgress, 100))];
            roomList.Add(availableRooms[RngManager.GetRandom(0, availableRooms.Count - 1)]);
            // Test specific boss.
            //var roomData = StageController.LoadRoomData().First(x => x.Name == "GG_Ghost_Gorb");
            //roomList.Insert(1, roomData);
            // Test specific room.
            //var selectedRoomData = StageController.LoadRoomData().First(x => x.Name == "Deepnest_01b");
            //roomList = [..selectedRoomData.AllowedEntrances.Select(x => new RoomData()
            //{
            //    Name = selectedRoomData.Name,
            //    SelectedTransition = x
            //}), .. roomList];
        }
        catch (Exception exception)
        {
            LogManager.Log("Error in setup run.", exception);
        }
        return roomList;
    }

    #endregion

    #region Private Methods

    private static Dictionary<int, Progress> RollAbilityRooms()
    {
        List<int> availableAbilityRooms = [.. Enumerable.Range(3, 90)];
        // 40 and 80 are reserved for fireball + quake (exclude the rooms before and after as well)
        for (int i = 38; i < 83; i++)
        {
            availableAbilityRooms.Remove(i);
            if (i == 42)
                i = 77;
        }
        bool fireballFirst = RngManager.GetRandom(0, 1) == 1;
        int dashRoom, clawRoom, wingRoom, cDashRoom, tearRoom, shadeCloakRoom, lanternRoom;
        dashRoom = RngManager.GetRandom(2, 30);
        availableAbilityRooms.RemoveAll(x => x >= dashRoom - 3 && x <= dashRoom + 3);
        List<int> clawSelection = [.. availableAbilityRooms.Where(x => x <= 30 && x >= 20)];
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
            { 40, fireballFirst ? Progress.Fireball : Progress.Quake },
            { 80, fireballFirst ? Progress.Quake : Progress.Fireball }
        };
    }

    #endregion
}
