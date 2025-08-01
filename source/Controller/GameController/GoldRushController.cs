using KorzUtils.Helper;
using Modding;
using System.Collections.Generic;
using System.Linq;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;

namespace TrialOfCrusaders.Controller.GameController;

internal class GoldRushController : GameModeController
{
    public override GameMode Mode => GameMode.GoldRush;

    public override string Explanation => "Have 2000 geo in your pocket and exit the current room to win. You may spend geo in the shop, but that might delay your freedom. Enemies will become stronger the more rooms you progress.";

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
                // Exclude bosses at the start and Gorgeous husk completely.
                if ((room.BossRoom && currentRoomIndex < 20) || room.Name == "Ruins_House_02")
                    continue;
                if (!addedRooms.Contains(room.Name))
                {
                    availableRooms.Add(room);
                    addedRooms.Add(room.Name);
                }
            }
        List<RoomData> selectedRooms = [];
        for (int i = 0; i < 10; i++)
        {
            RoomData nextRoom = availableRooms[RngManager.GetRandom(0, availableRooms.Count - 1)];
            if (nextRoom.BossRoom)
                availableRooms.RemoveAll(x => x.BossRoom);
            else
                availableRooms.Remove(nextRoom);
            selectedRooms.Add(nextRoom);
        }

        // Check if all items have been obtained.
        if ((int)progress != 511)
        {
            List<Progress> values = [..(System.Enum.GetValues(typeof(Progress)) as Progress[])];
            if (!progress.HasFlag(Progress.Dash))
                values.Remove(Progress.ShadeCloak);
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
        if (PDHelper.Geo > 2000)
            return true;
        if (ControllerShorthands.StageRef.CurrentRoomData.Count - 1 == ControllerShorthands.StageRef.CurrentRoomNumber)
            ControllerShorthands.StageRef.CurrentRoomData.AddRange(GenerateRoomList(false));
        return false;
    }

    public override void OnStart() => ModHooks.SetPlayerIntHook += ModHooks_SetPlayerIntHook;

    public override void OnEnd() => ModHooks.SetPlayerIntHook -= ModHooks_SetPlayerIntHook;

    public override void SetupTreasurePool()
    {
        List<string> treasures = [.. TreasureManager.Powers.Select(x => x.Name)];
        // Credit would defeat the purpose of the mode.
        treasures.Remove("Credit");
        TreasureManager.TreasurePool = treasures;
    }

    private int ModHooks_SetPlayerIntHook(string name, int orig)
    {
        if (name == nameof(PlayerData.geo))
        {
            if (orig >= 2001)
                TrialOfCrusaders.Holder.StartCoroutine(ControllerShorthands.StageRef.InitiateTransition());
            return orig;
        }
        return orig;
    }
}
