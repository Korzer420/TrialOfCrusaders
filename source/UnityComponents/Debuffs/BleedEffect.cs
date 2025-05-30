using KorzUtils.Helper;
using UnityEngine;

namespace TrialOfCrusaders.UnityComponents.Debuffs;

internal class BleedEffect : MonoBehaviour
{
    private float _leftDuration = 5f;
    private float _animationTimer = 0.25f;
    public const string TextColor = "#ff0303";

    private HealthManager _enemy;

    public static GameObject Prefab { get; set; }

    void Start() => _enemy = GetComponent<HealthManager>();

    void FixedUpdate()
    {
        if (_leftDuration <= 0 || _enemy == null || !_enemy.gameObject.activeSelf || _enemy.isDead)
            Destroy(this);
        if (_leftDuration != 5f && !_enemy.IsInvincible && ((int)_leftDuration != (int)(_leftDuration - Time.deltaTime) || _leftDuration - Time.deltaTime <= 0f))
            _enemy.ApplyExtraDamage(Mathf.Max(1, PDHelper.NailDamage));
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
    internal static void PreparePrefab(GameObject prefab)
    {
        prefab.name = "Bleed Effect";
        prefab.GetComponent<SpriteRenderer>().color = Color.red;
        Prefab = prefab;
        GameObject.DontDestroyOnLoad(Prefab);
    }

}