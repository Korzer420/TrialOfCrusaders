using KorzUtils.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;
using TrialOfCrusaders.Resources.Text;

namespace TrialOfCrusaders.Controller.GameController;

internal class CrusaderController : GameModeController
{
    public override GameMode Mode => GameMode.Crusader;

    public override string Explanation => LobbyDialog.ExplanationTrialCrusader;

    public override List<RoomData> GenerateRoomList(bool atStart)
    {
        List<RoomData> roomList = [];
        List<RoomData> availableRooms = StageController.LoadRoomData();
        Progress currentProgress = Progress.Lantern | Progress.Tear | Progress.CrystalHeart;
        bool fireballFirst = RngManager.GetRandom(0, 1) == 0;
        Dictionary<int, Progress> progressItemRooms = new()
            {
                { 4, Progress.Dash },
                { 9, fireballFirst ? Progress.Fireball : Progress.Quake },
                { 14, Progress.Claw },
                { 19, Progress.Wings },
                { 29, fireballFirst ? Progress.Quake : Progress.Fireball },
                { 39, Progress.ShadeCloak },
            };
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
                if (room.Available(false, currentProgress))
                {
                    if (room.BossRoom && currentRoom > 20 && ((currentRoom <= lastBossRoom + 2 && currentRoom % 13 != 0) || currentRoom < 13 || currentRoom == 49))
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
        availableRooms = [.. availableRooms.Where(x => x.BossRoom && !x.Available(false, currentProgress))];
        roomList.Add(availableRooms[RngManager.GetRandom(0, availableRooms.Count - 1)]);
        //roomList.Add(availableRooms.First(x => x.Name == "GG_Radiance"));

        // Test specific boss.
        //var roomData = StageRef.LoadRoomData().First(x => x.Name == "GG_Ghost_Gorb");
        //roomList.Insert(1, roomData);
        //Test specific room.
        //var selectedRoomData = StageController.LoadRoomData().Where(x => x.Name == "Mines_24");
        //Test room at start.
        //roomList = [..selectedRoomData, ..roomList];
        // Test room insert
        //roomList.Insert(30, new RoomData()
        //{
        //    SelectedTransition = "right1",
        //    Name = selectedRoomData.Name
        //});
        return roomList;
    }

    public override void OnStart()
    {
        PDHelper.HasLantern = true;
        PDHelper.HasSuperDash = true;
        PDHelper.HasAcidArmour = true;
    }

    public override bool CheckForEnding() => ControllerShorthands.StageRef.CurrentRoomData.Count - 1 == ControllerShorthands.StageRef.CurrentRoomIndex;
}
