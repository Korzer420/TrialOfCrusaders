using KorzUtils.Helper;
using Modding;
using System.Collections;
using System.Collections.Generic;
using TrialOfCrusaders.Controller;
using UnityEngine;

namespace TrialOfCrusaders.UnityComponents;

internal class Gate : MonoBehaviour
{
    private List<GameObject> _blockers = [];

    public static GameObject Prefab { get; set; }

    void Start()
    {
        if (GetComponent<TransitionPoint>().isADoor)
            return;
        Vector2 elementPosition = transform.position;

        //if (GetComponent<GateSnap>() is GateSnap gateSnap)
        //{
        //    float snapX = ReflectionHelper.GetField<GateSnap, float>(gateSnap, "snapX");
        //    float snapY = ReflectionHelper.GetField<GateSnap, float>(gateSnap, "snapY");
        //    elementPosition = new Vector2(Mathf.Round(elementPosition.x / snapX) * snapX, Mathf.Round(elementPosition.y / snapY) * snapY);
        //}
        GetComponent<BoxCollider2D>().enabled = false;
        int direction = 0;
        if (gameObject.name.Contains("top"))
            direction = 1;
        else if (gameObject.name.Contains("right"))
            direction = 2;
        else if (gameObject.name.Contains("left"))
            direction = 3;
        // Exceptions which should receive a hazard respawn to prevent softlocks.
        if ((gameObject.scene.name == "Cliffs_02" && (gameObject.name == "right1" || gameObject.name == "bot2"))
            || (gameObject.scene.name == "Deepnest_East_07" && gameObject.name.Contains("bot"))
            || (gameObject.scene.name == "Mines_34" && gameObject.name != "bot1")
            || (gameObject.scene.name == "Fungus2_30" && gameObject.name == "bot1"))
        {
            GameObject blocker = new("TrialBlocker");
            blocker.transform.localScale = transform.localScale;
            blocker.transform.position = elementPosition;
            blocker.AddComponent<BoxCollider2D>().size = GetComponent<BoxCollider2D>().size;
            blocker.GetComponent<BoxCollider2D>().isTrigger = true;
            blocker.AddComponent<RespawnZone>();
            blocker.SetActive(true);
            _blockers.Add(blocker);
            blocker.transform.eulerAngles = direction switch
            {
                0 => new Vector3(0f, 0f, 90f),
                1 => new Vector3(0f, 0f, 270f),
                2 => new Vector3(0f, 0f, 180f),
                _ => new Vector3(0f, 0f, 0f)
            };
        }
        // Special case as this transition is placed weirdly.
        else if (gameObject.scene.name == "Crossroads_01" && gameObject.name == "top1")
            return;
        else
        {
            int realHeight = Mathf.CeilToInt(GetComponent<BoxCollider2D>().size.y * transform.localScale.y);
            if (realHeight % 4 != 0)
                realHeight += 4 - (realHeight % 4);
            bool evenAmount = realHeight % 8 == 0;
            for (; realHeight > 0; realHeight -= 4)
            {
                GameObject blocker = GameObject.Instantiate(Prefab);
                blocker.name = "TrialBlocker";
                blocker.transform.localScale = new(1f, 1f);
                blocker.SetActive(true);
                blocker.GetComponent<BoxCollider2D>().enabled = false;
                blocker.GetComponent<BoxCollider2D>().offset = new(2000f, 2000f);
                blocker.LocateMyFSM("Control").GetState("Init").RemoveActions(1);
                // Remove camera lock.
                foreach (Transform child in blocker.transform)
                    GameObject.Destroy(child.gameObject);
                int platformAmount = _blockers.Count;
                Vector3 position;
                if (evenAmount)
                {
                    if (direction < 2)
                        position = new(elementPosition.x + (-2 + (platformAmount % 2 == 1 ? 4 : -4) * Mathf.CeilToInt(platformAmount / 2f)), elementPosition.y);
                    else
                        position = new(elementPosition.x, elementPosition.y + (-2 + (platformAmount % 2 == 1 ? 4 : -4) * Mathf.CeilToInt(platformAmount / 2f)));
                }
                else
                {
                    if (platformAmount == 0)
                        position = elementPosition;
                    else if (direction < 2)
                        position = new(elementPosition.x + ((platformAmount % 2 == 1 ? 4 : -4) * Mathf.CeilToInt(platformAmount / 2f)), elementPosition.y);
                    else
                        position = new(elementPosition.x, elementPosition.y + ((platformAmount % 2 == 1 ? 4 : -4) * Mathf.CeilToInt(platformAmount / 2f)));
                }

                switch (direction)
                {
                    case 0:
                        blocker.transform.eulerAngles = new Vector3(0f, 0f, 90f);
                        position += new Vector3(0f, 1f);
                        break;
                    case 1:
                        blocker.transform.eulerAngles = new Vector3(0f, 0f, 270f);
                        position -= new Vector3(0f, 1f);
                        break;
                    case 2:
                        blocker.transform.eulerAngles = new Vector3(0f, 0f, 180f);
                        position -= new Vector3(1f, 0f);
                        break;
                    default:
                        blocker.transform.eulerAngles = new Vector3(0f, 0f, 0f);
                        position += new Vector3(1f, 0f);
                        break;
                }
                if (position.x < 1f)
                    position.x = 1f;
                else if (position.x > GameManager.instance.sceneWidth)
                    position.x -= 1f;

                if (position.y < 1f)
                    position.y = 1f;
                else if (position.y > GameManager.instance.sceneHeight)
                    position.y -= 1f;
                blocker.transform.position = position + new Vector3(0f, 0f, 0.1f);
                _blockers.Add(blocker);
            }
        }
    }

    internal void PlaceCollider()
    {
        foreach (var blocker in _blockers)
            blocker.GetComponent<BoxCollider2D>().offset = new(0f, 0f);
    }

    internal void Revert()
    {
        if (GetComponent<TransitionPoint>().isADoor)
            return;
        GetComponent<BoxCollider2D>().enabled = !StageController.UpcomingTreasureRoom;
        foreach (GameObject item in _blockers)
            GameObject.Destroy(item);
    }

    internal IEnumerator WaitForHero()
    {
        yield return new WaitUntil(() => HeroController.instance.transform.position.y < _blockers[0].transform.position.y - 1);
        PlaceCollider();
    }
}
