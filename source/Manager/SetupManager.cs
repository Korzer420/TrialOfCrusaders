﻿using System;
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

    internal static List<RoomData> GenerateRun(GameMode gameMode) => gameMode switch
    {
        GameMode.Crusader => GenerateCrusadersRun(),
        GameMode.GrandCrusader => GenerateGrandCrusaderRun(),
        _ => throw new NotImplementedException("Not implemented game mode."),
    };

    internal static List<RoomData> GenerateCrusadersRun()
    {
        List<RoomData> roomList = [];
        try
        {
            List<RoomData> availableRooms = LoadRoomData();
            Progress currentProgress = Progress.Lantern | Progress.Tear | Progress.CrystalHeart;
            Dictionary<int, Progress> progressItemRooms = RollAbilityRooms(GameMode.Crusader);
            // Used to prevent room repeats under 15 rooms.
            List<string> lastRooms = [];
            int lastBossRoom = -1;
            for (int currentRoom = 0; currentRoom < 50; currentRoom++)
            {
                // Ability rooms/treasure rooms are not part of the normal routine.
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
                        if (room.BossRoom && ((currentRoom <= lastBossRoom + 2 && currentRoom % 13 != 0) || currentRoom < 13))
                            continue;
                        else if ((!room.BossRoom && currentRoom % 13 == 0 && currentRoom != 0) || lastRooms.Contains(room.Name))
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
            availableRooms = [.. availableRooms.Where(x => x.BossRoom && !x.Available(false, currentProgress, 100))];
            roomList.Add(availableRooms[RngManager.GetRandom(0, availableRooms.Count - 1)]);
            //roomList.Add(availableRooms.First(x => x.Name == "GG_Radiance"));

            // Test specific boss.
            //var roomData = StageController.LoadRoomData().First(x => x.Name == "GG_Ghost_Gorb");
            //roomList.Insert(1, roomData);
            //Test specific room.
            //var selectedRoomData = StageController.LoadRoomData().First(x => x.Name == "Tutorial_01");
            //Test room at start.
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

    internal static List<RoomData> GenerateGrandCrusaderRun()
    {
        List<RoomData> roomList = [];
        try
        {
            List<RoomData> availableRooms = LoadRoomData();
            Progress currentProgress = Progress.None;
            Dictionary<int, Progress> progressItemRooms = RollAbilityRooms(GameMode.GrandCrusader);
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
                    if (room.Available(false, currentProgress, currentRoom))
                    {
                        if (!room.BossRoom && (lastRooms.Contains(room.Name) || (currentRoom > 0 && currentRoom % 20 == 0)))
                            continue;
                        else if (room.BossRoom && (currentRoom < 20 || (currentRoom <= lastBossRoom + 2 && currentRoom % 20 != 0)))
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
            availableRooms = [.. availableRooms.Where(x => x.BossRoom && !x.Available(false, currentProgress, 100))];
            roomList.Add(availableRooms[RngManager.GetRandom(0, availableRooms.Count - 1)]);

            // Test specific boss.
            //var roomData = StageController.LoadRoomData().First(x => x.Name == "GG_Dung_Defender");
            //roomList.Insert(91, roomData);
            // Test specific room.
            //var selectedRoomData = StageController.LoadRoomData().First(x => x.Name == "Fungus2_14");
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

    #endregion

    #region Private Methods

    private static List<RoomData> LoadRoomData()
    {
        // Add each normal room 5 times so each one has the same probability regardless of available entrances.
        // For bosses we only take two (although they only can appear once).
        return [..StageController.LoadRoomData()
                .SelectMany(x =>
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
    }

    private static Dictionary<int, Progress> RollAbilityRooms(GameMode gameMode)
    {
        bool fireballFirst = RngManager.GetRandom(0, 1) == 1;
        if (gameMode == GameMode.GrandCrusader)
        {
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
        else if (gameMode == GameMode.Crusader)
            return new()
            {
                { 4, Progress.Dash },
                { 9, fireballFirst ? Progress.Fireball : Progress.Quake },
                { 14, Progress.Claw },
                { 19, Progress.Wings },
                { 29, fireballFirst ? Progress.Quake : Progress.Fireball },
                { 39, Progress.ShadeCloak },
            };
        else
            throw new NotImplementedException();
    }

    #endregion
}
