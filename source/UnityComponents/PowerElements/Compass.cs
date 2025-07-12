using KorzUtils.Helper;
using TrialOfCrusaders.UnityComponents.CombatElements;
using UnityEngine;
using static TrialOfCrusaders.ControllerShorthands;

namespace TrialOfCrusaders.UnityComponents.PowerElements;

internal class Compass : MonoBehaviour
{
    private GameObject _arrow;
    private bool _initialized;

    void Start()
    {
        _arrow = new("Arrow");
        _arrow.transform.SetParent(transform);
        _arrow.layer = 5;
        _arrow.transform.localPosition = new(0f, 0f, -0.1f);
        _arrow.transform.localScale = new(1.3f, 1.3f);
        _arrow.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Other.Arrow");
        _arrow.SetActive(true);
    }

    void FixedUpdate()
    {
        try
        {
            if (!CombatRef.InCombat || StageRef.CurrentRoom.BossRoom)
                GameObject.Destroy(gameObject);
            else
            {
                Vector3 nearestLocation = Vector3.zero;
                Vector3 heroPosition = HeroController.instance.transform.position;
                float nearestDistance = float.MaxValue;
                if (CombatRef.ActiveEnemies.Count > 0)
                    foreach (HealthManager enemy in CombatRef.ActiveEnemies)
                    {
                        if (enemy == null || enemy.gameObject == null 
                            || enemy.isDead || !enemy.gameObject.activeSelf || enemy.GetComponent<BaseEnemy>() == null)
                            continue;
                        float distance = Vector3.Distance(heroPosition, enemy.transform.position);
                        if (distance < nearestDistance)
                        {
                            nearestDistance = distance;
                            nearestLocation = enemy.transform.position;
                        }    
                    }
                if (nearestDistance != float.MaxValue)
                {
                    Vector3 distance = nearestLocation - heroPosition;
                    distance.z = 0;
                    float angle = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;
                    _arrow.transform.SetRotation2D(angle);
                }
            }
        }
        catch (System.Exception ex)
        {
            LogHelper.Write("Error", ex);
        }
    }
}
