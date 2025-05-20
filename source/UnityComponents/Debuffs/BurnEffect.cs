using TrialOfCrusaders.Powers.Uncommon;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace TrialOfCrusaders.UnityComponents.Debuffs;

/// <summary>
/// A dot that deals all damage over the course of 10 seconds (10% per seconds).
/// <para/>New damage refreshes the duration and is added onto the leftover damage.
/// </summary>
internal class BurnEffect : MonoBehaviour
{
    private bool _initialDamage = false;
    private GameObject _flameParticle;
    private HealthManager _enemy;

    private float _leftDuration = 10f;

    public int LeftDamage { get; set; }

    public static GameObject Burn { get; set; }

    internal static bool PoweredUp => CombatController.HasPower(out FragileSpirit spirit) && spirit.SpiritActive;

    public const string TextColor = "#e6a100";

    void Start()
    {
        _flameParticle = Instantiate(Burn, transform);
        _flameParticle.transform.localPosition = new(0f, 0f, -1f);
        _enemy = GetComponent<HealthManager>();
        ParticleSystem particle = _flameParticle.GetComponent<ParticleSystem>();
        MainModule mainModule = particle.main;
        mainModule.duration = 10;
        mainModule.loop = true;
        mainModule.maxParticles *= 3;
        MinMaxCurve speed = mainModule.startSpeed;
        speed.constantMin *= 3;
        speed.constantMax *= 3;
        _flameParticle.SetActive(true);
        particle.Play();
    }

    void FixedUpdate()
    {
        // Prevent destroying the component if the first damage has not been applied yet.
        if (!_initialDamage)
            return;
        // Check if a second barrier has been passed.
        if (_leftDuration != 10f && ((int)_leftDuration != (int)(_leftDuration - Time.deltaTime) || _leftDuration - Time.deltaTime <= 0f))
        {
            int modifier = (int)_leftDuration + 1;
            int damage = Mathf.CeilToInt(LeftDamage / modifier);
            // Respect invinciblity
            if (!_enemy.IsInvincible)
                _enemy.ApplyExtraDamage(PoweredUp ? damage * 2 : damage);
            LeftDamage -= damage;
        }
        _leftDuration -= Time.deltaTime;
        if (_leftDuration <= 0 || _enemy.isDead || LeftDamage <= 0)
        {
            Destroy(_flameParticle);
            Destroy(this);
        }
    }

    public void AddDamage(int damage)
    {
        LeftDamage += damage;
        _leftDuration = 10f;
        _initialDamage = true;
    }
}
