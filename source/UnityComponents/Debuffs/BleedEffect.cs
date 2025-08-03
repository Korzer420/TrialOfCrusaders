using KorzUtils.Helper;
using TrialOfCrusaders.Manager;
using UnityEngine;
using static TrialOfCrusaders.ControllerShorthands;

namespace TrialOfCrusaders.UnityComponents.Debuffs;

internal class BleedEffect : MonoBehaviour
{
    private float _leftDuration = 5f;
    private float _animationTimer = 0.25f;
    public const string TextColor = "#ff0303";
    public const int BleedFlagDamage = 6083;

    private HealthManager _enemy;

    public static GameObject Prefab { get; set; }

    void Start() => _enemy = GetComponent<HealthManager>();

    void FixedUpdate()
    {
        try
        {
            if (_leftDuration <= 0 || _enemy == null || !_enemy.gameObject.activeSelf || _enemy.isDead)
                Destroy(this);
            if (_leftDuration != 5f && ((int)_leftDuration != (int)(_leftDuration - Time.deltaTime) || _leftDuration - Time.deltaTime <= 0f)
                && _enemy != null && _enemy.hp > 0)
                _enemy.ApplyExtraDamage(Mathf.Max(1, PDHelper.NailDamage / (PowerRef.DebuffsStronger ? 1 : 2)));
            _leftDuration -= Time.deltaTime;
            _animationTimer -= Time.deltaTime;
            if (_animationTimer <= 0f)
            {
                _animationTimer = 0.25f;
                // This does recycle itself
                GameObject bleedEffect = Instantiate(Prefab, transform.position - new Vector3(0f, 0f, 1f), Quaternion.identity);
                bleedEffect.SetActive(true);
            }
        }
        catch (System.Exception ex)
        {
            LogManager.Log("Error in bleed update: ", ex);
        }
    }
    internal static void PreparePrefab(GameObject prefab)
    {
        Prefab = GameObject.Instantiate(prefab);
        Prefab.name = "Bleed Effect";
        Prefab.GetComponent<SpriteRenderer>().color = Color.red;
    }
}