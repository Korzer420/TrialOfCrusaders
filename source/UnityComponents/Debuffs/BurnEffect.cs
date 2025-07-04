using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Manager;
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

    public static GameObject Prefab { get; set; }

    public const string TextColor = "#e6a100";

    void Start()
    {
        _flameParticle = Instantiate(Prefab, transform);
        _flameParticle.transform.localPosition = new(0f, 0f, -1f);
        _enemy = GetComponent<HealthManager>();
        _flameParticle.SetActive(true);
    }

    void FixedUpdate()
    {
        try
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
                if (_enemy != null && _enemy.hp > 0)
                    _enemy.ApplyExtraDamage(CombatController.DebuffsStronger ? Mathf.FloorToInt(damage * 1.5f) : damage);
                LeftDamage -= damage;
            }
            _leftDuration -= Time.deltaTime;
            if (_leftDuration <= 0 || _enemy.isDead || LeftDamage <= 0)
            {
                Destroy(_flameParticle);
                Destroy(this);
            }
        }
        catch (System.Exception ex)
        {
            LogManager.Log("Error in burn update: ", ex);
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

    internal static void PreparePrefab(GameObject original)
    {
        GameObject prefab = GameObject.Instantiate(original);
        prefab.name = "Burn Effect";
        ParticleSystem particle = prefab.GetComponent<ParticleSystem>();
        MainModule mainModule = particle.main;
        mainModule.duration = 10;
        mainModule.loop = true;
        mainModule.maxParticles *= 10;
        mainModule.playOnAwake = true;
        mainModule.startColor = new(new Color(0.9f, 0.4f, 0f)); // Orange
        particle.emissionRate = 100f;
        Prefab = prefab;
    }
}
