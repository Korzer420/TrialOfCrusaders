using GlobalEnums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;
using UnityEngine;

namespace TrialOfCrusaders.Controller;

/// <summary>
/// Controls the respawn in the starting room.
/// </summary>
public class SpawnController : BaseController
{
    private string _warpScene;

    //public bool ContinueSpawn { get; set; }

    public override Phase[] GetActivePhases() => [Phase.Run, Phase.Result, Phase.Lobby];

    protected override void Enable()
    {
        LogManager.Log("Enable Spawn Controller");
        PDHelper.RespawnMarkerName = "SpawnPoint";
        PDHelper.RespawnScene = "Room_Colosseum_01";
        //PDHelper.RespawnScene = ContinueSpawn 
        //        ? "Room_Colosseum_01"
        //        : "Room_nailmaster";
        PlayerData.instance.mapZone = MapZone.COLOSSEUM;
        PDHelper.RespawnType = 2;
        PDHelper.RespawnFacingRight = true; //!ContinueSpawn;
        PDHelper.HazardRespawnFacingRight = true;
        On.GameManager.BeginSceneTransition += GameManager_BeginSceneTransition;
        On.HeroController.LocateSpawnPoint += HeroController_LocateSpawnPoint;
        On.GameManager.GetCurrentMapZone += PreventDreamRespawn;
    }

    protected override void Disable()
    {
        LogManager.Log("Disable Spawn Controller");
        On.GameManager.BeginSceneTransition -= GameManager_BeginSceneTransition;
        On.HeroController.LocateSpawnPoint -= HeroController_LocateSpawnPoint;
        On.GameManager.GetCurrentMapZone -= PreventDreamRespawn;
    }

    private Transform HeroController_LocateSpawnPoint(On.HeroController.orig_LocateSpawnPoint orig, HeroController self)
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
        //gameObject.transform.position = ContinueSpawn 
        //    ? new(19.52f, 4.4f)
        //    : new(15.95f, 6.4f);
        gameObject.AddComponent<RespawnMarker>().respawnFacingRight = true;//!ContinueSpawn;
        gameObject.SetActive(true);

        //if (ContinueSpawn)
        //{
        //    GameObject gate = GameObject.Instantiate(Gate.Prefab);
        //    gate.transform.position = new(26.33f, 5.4f);
        //    gate.SetActive(true);
        //    gate.transform.SetRotation2D(180f);
        //    CoroutineHelper.WaitFrames(() => PlayMakerFSM.BroadcastEvent("DREAM GATE CLOSE"), true);
        //    CoroutineHelper.WaitForHero(() => ContinueSpawn = false, true);
        //}

        return gameObject.transform;
    }

    private void GameManager_BeginSceneTransition(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
    {
        if (GameManager.instance?.RespawningHero == true)
        {
            PlayerData.instance.SetInt(nameof(PlayerData.instance.respawnType), 2);
            info.SceneName = "Room_Colosseum_01";
            //info.SceneName = ContinueSpawn 
            //    ? "Room_nailmaster" 
            //    : "Room_Colosseum_01";
            info.EntryGateName = "left1";
            _warpScene = info.SceneName;
            //if (ContinueSpawn)
            //    StageRef.Initialize();
        }
        orig(self, info);
    }

    private string PreventDreamRespawn(On.GameManager.orig_GetCurrentMapZone orig, GameManager self)
    {
        string value = orig(self);
        if (value == "GODS_GLORY" || value == "WHITE_PALACE" || value == "DREAM_WORLD")
            return "CROSSROADS";
        return value;
    }
}