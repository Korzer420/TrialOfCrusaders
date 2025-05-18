using GlobalEnums;
using KorzUtils.Helper;
using UnityEngine;

namespace TrialOfCrusaders;

/// <summary>
/// Controls the respawn in the starting room.
/// </summary>
internal static class Spawner
{
    private static string _warpScene;

    internal static void Load()
    {
        PDHelper.RespawnMarkerName = "SpawnPoint";
        PDHelper.RespawnScene = "Room_Colosseum_01";
        PlayerData.instance.mapZone = MapZone.COLOSSEUM;
        PDHelper.RespawnType = 2;
        PDHelper.RespawnFacingRight = true;
        PDHelper.HazardRespawnFacingRight = true;
        PDHelper.ColosseumBronzeOpened = true;
        On.GameManager.BeginSceneTransition += GameManager_BeginSceneTransition;
        On.HeroController.LocateSpawnPoint += HeroController_LocateSpawnPoint;
    }

    internal static void Unload()
    {
        On.GameManager.BeginSceneTransition -= GameManager_BeginSceneTransition;
        On.HeroController.LocateSpawnPoint -= HeroController_LocateSpawnPoint;
    }

    private static Transform HeroController_LocateSpawnPoint(On.HeroController.orig_LocateSpawnPoint orig, HeroController self)
    {
        Transform spawnPoint = orig(self);
        if (string.IsNullOrEmpty(_warpScene))
            return spawnPoint;
        // Mock a respawn point.
        GameObject gameObject = new("SpawnPoint")
        {
            tag = "RespawnPoint"
        };
        gameObject.transform.position = new(15.95f, 6.4f);
        gameObject.AddComponent<RespawnMarker>().respawnFacingRight = true;
        gameObject.SetActive(true);
        return gameObject.transform;
    }

    private static void GameManager_BeginSceneTransition(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
    {
        if (GameManager.instance?.RespawningHero == true)
        {
            PlayerData.instance.SetInt(nameof(PlayerData.instance.respawnType), 2);
            info.SceneName = "Room_Colosseum_01";
            info.EntryGateName = "left1";
            _warpScene = info.SceneName;
        }
        orig(self, info);
    }
}