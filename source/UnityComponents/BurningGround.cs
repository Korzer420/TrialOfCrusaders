using KorzUtils.Helper;
using Modding.Utils;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.UnityComponents.Debuffs;
using UnityEngine;

namespace TrialOfCrusaders.UnityComponents;

internal class BurningGround : MonoBehaviour
{
    private BoxCollider2D _hitbox;
    private float _igniteCooldown = 0f;
    private float _duration = 5f;

    void Start()
    {
        _hitbox = gameObject.AddComponent<BoxCollider2D>();
        _hitbox.size = new(11.5f, 5f);
        _hitbox.isTrigger = true;
        for (int i = 0; i < 9; i++)
            SpawnFlames(i);
    }

    void FixedUpdate()
    {
        if (_igniteCooldown > 0f)
            _igniteCooldown -= Time.deltaTime;
        _duration -= Time.deltaTime;
        if (_duration <= 0f)
            Destroy(gameObject);
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.GetComponent<HealthManager>() == null)
        {
            Physics2D.IgnoreCollision(_hitbox, col);
            return;
        }
        if (_igniteCooldown > 0f)
            return;
        _igniteCooldown = 1.5f;
        col.gameObject.GetOrAddComponent<BurnEffect>().AddDamage(100 + (CombatController.SpiritLevel * 2));
    }

    private void SpawnFlames(int count)
    {
        GameObject flame = GameObject.Instantiate(BurnEffect.Prefab, transform);
        flame.name = "Ground Flame " + count;
        flame.transform.localPosition = new(-5f + 1.25f * count, 0f);
        flame.SetActive(true);
    }
}
