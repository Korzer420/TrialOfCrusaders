using KorzUtils.Helper;
using UnityEngine;

namespace TrialOfCrusaders.UnityComponents.Debuffs;

internal class BleedEffect : MonoBehaviour
{
    private float _leftDuration = 5f;
    public const string TextColor = "#ff0303";

    private HealthManager _enemy;

    public static GameObject Bleed { get; set; }

    void Start() => _enemy = GetComponent<HealthManager>();

    void FixedUpdate()
    {
        if (_leftDuration <= 0 || _enemy.isDead)
            Destroy(this);
        if (_leftDuration != 5f && ((int)_leftDuration != (int)(_leftDuration - Time.deltaTime) || _leftDuration - Time.deltaTime <= 0f))
        {
            // This does recycle itself
            GameObject bleedEffect = Instantiate(Bleed, transform.position - new Vector3(0f, 0f, 1f), Quaternion.identity);
            bleedEffect.GetComponent<SpriteRenderer>().color = Color.red;
            bleedEffect.SetActive(true);
            if (!_enemy.IsInvincible)
                _enemy.ApplyExtraDamage(Mathf.Max(1, PDHelper.NailDamage / 5));
        }
        _leftDuration -= Time.deltaTime;
    }
}