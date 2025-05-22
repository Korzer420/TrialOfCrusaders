using System.Collections.Generic;
using System.Linq;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders;

internal static class SetupManager
{
    internal static List<RoomData> GenerateNormalRun(int seed)
    {
        //return StageController.RoomList.Where(x => !x.BossRoom)
        //    .Select(x => new RoomData()
        //{
        //    SelectedTransition = x.AllowedEntrances[0],
        //    Name = x.Name
        //})?.ToList();
        // Run rules:
        // 120 rooms. (Around 30 min - 1 hour)
        // No boss before Room 20.
        // Guarantee a boss in room 120.
        // No bosses/Items in between 3 rooms of each other.
        // Claw within the first 30 rooms.
        // Dash within the first 50 rooms.
        // Shade Cloak within 100 rooms (after dash)
        // Forced treasure room at 40 and 80 with fireball/dive (if already obtained, spawn a rare treasure instead).
        // No room repeat under 15 rooms.
        // No boss repeat.
        RngProvider.Seed = seed;
        List<RoomData> roomList = [];
        //roomList.Add(StageController.RoomList.First(x => x.Name == "Crossroads_39"));
        //roomList[0].SelectedTransition = "left1";
        // Add each normal room 5 times so each one has the same probability regardless of available entrances.
        // For bosses we only take one though.
        List<RoomData> availableRooms = [..StageController.RoomPool.SelectMany(x =>
        {
            if (x.BossRoom)
                return [x];
            List<RoomData> roomCopies = [..x.AllowedEntrances.Select(y => new RoomData()
            {
                Name = x.Name,
                ConditionalProgress = x.ConditionalProgress,
                NeededProgress = x.NeededProgress,
                EasyConditionalProgress = x.EasyConditionalProgress,
                EasyNeededProgress = x.EasyNeededProgress,
                SelectedTransition = y
            })];
            if (roomCopies.Count != 5)
                for (int i = roomCopies.Count; i <= 5; i++)
                    roomCopies.Add(new RoomData()
                    {
                        Name = x.Name,
                        ConditionalProgress = x.ConditionalProgress,
                        NeededProgress = x.NeededProgress,
                        EasyConditionalProgress = x.EasyConditionalProgress,
                        EasyNeededProgress = x.EasyNeededProgress,
                        SelectedTransition = x.AllowedEntrances[RngProvider.GetRandom(0, x.AllowedEntrances.Count - 1)]
                    });
            return roomCopies;
        })];

        Progress currentProgress = Progress.None;
        Dictionary<int, Progress> progressItemRooms = RollAbilityRooms();

        // Used to prevent room repeats under 15 rooms.
        List<string> lastRooms = [];
        for (int currentRoom = 0; currentRoom < 120; currentRoom++)
        {
            // Ability rooms/Treasure rooms are not part of the normal routine.
            // The latter will be generated on the spot, but since the abilities open more rooms, we calculate them before hand.
            if (progressItemRooms.ContainsKey(currentRoom))
            {
                roomList.Add(new() { Name = progressItemRooms[currentRoom].ToString(), SelectedTransition = "Warp" });
                currentProgress |= progressItemRooms[currentRoom];
            }
            List<RoomData> reachableRooms = [];
            foreach (RoomData room in availableRooms)
                if (room.Available(false, currentProgress, currentRoom))
                {
                    // For none boss rooms ensure the same room cannot occur between 15 rooms of each other.
                    // For boss rooms ensure that no boss appear
                    if ((!room.BossRoom && lastRooms.Contains(room.Name))
                        || (room.BossRoom && (roomList[roomList.Count - 1].BossRoom || roomList[roomList.Count - 2].BossRoom) && currentRoom % 20 != 0))
                        continue;
                    reachableRooms.Add(room);
                }
            RoomData rolledRoom = reachableRooms[RngProvider.GetRandom(0, reachableRooms.Count - 1)];
            availableRooms.Remove(rolledRoom);
            lastRooms.Add(rolledRoom.Name);
            roomList.Add(rolledRoom);
            if (lastRooms.Count == 16)
                lastRooms.RemoveAt(0);
        }
        // This should only leave NKG, Pure Vessel and Radiance which we use as end bosses.
        availableRooms = [.. availableRooms.Where(x => x.BossRoom && !x.Available(false, currentProgress, 120))];
        roomList.Add(availableRooms[RngProvider.GetRandom(0, availableRooms.Count - 1)]);
        return roomList;
    }

    private static Dictionary<int, Progress> RollAbilityRooms()
    {
        List<int> availableAbilityRooms = [.. Enumerable.Range(3, 107)];
        // 40 and 80 are reserved for fireball + quake (exclude the rooms before and after as well)
        for (int i = 38; i < 83; i++)
        {
            availableAbilityRooms.Remove(i);
            if (i == 43)
                i = 77;
        }
        bool fireballFirst = RngProvider.GetRandom(0, 1) == 1;
        int dashRoom, clawRoom, wingRoom, cDashRoom, tearRoom, shadeCloakRoom, lanternRoom;
        dashRoom = RngProvider.GetRandom(2, 60);
        while (dashRoom > 37 && dashRoom < 43)
            dashRoom = RngProvider.GetRandom(2, 60);
        availableAbilityRooms.RemoveAll(x => x >= dashRoom - 3 && x <= dashRoom + 3);
        // Claw should be forced fairly early (as it opens many rooms)
        List<int> clawSelection = [.. availableAbilityRooms.Where(x => x <= 30)];
        clawRoom = clawSelection[RngProvider.GetRandom(0, clawSelection.Count - 1)];
        availableAbilityRooms.RemoveAll(x => x >= clawRoom - 3 && x <= clawRoom + 3);
        wingRoom = availableAbilityRooms[RngProvider.GetRandom(0, availableAbilityRooms.Count - 1)];
        availableAbilityRooms.RemoveAll(x => x >= wingRoom - 3 && x <= wingRoom + 3);
        cDashRoom = availableAbilityRooms[RngProvider.GetRandom(0, availableAbilityRooms.Count - 1)];
        availableAbilityRooms.RemoveAll(x => x >= cDashRoom - 3 && x <= cDashRoom + 3);
        tearRoom = availableAbilityRooms[RngProvider.GetRandom(0, availableAbilityRooms.Count - 1)];
        availableAbilityRooms.RemoveAll(x => x >= tearRoom - 3 && x <= tearRoom + 3);
        lanternRoom = availableAbilityRooms[RngProvider.GetRandom(0, availableAbilityRooms.Count - 1)];
        availableAbilityRooms.RemoveAll(x => x >= lanternRoom - 3 && x <= lanternRoom + 3);
        // Shade cloak is only available after dash
        availableAbilityRooms.RemoveAll(x => x < dashRoom + 3);
        shadeCloakRoom = availableAbilityRooms[RngProvider.GetRandom(0, availableAbilityRooms.Count - 1)];
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
}
