using System;
using UnityEngine;

namespace TrialOfCrusaders.UnityComponents;

internal class WeakendEffect : MonoBehaviour
{
    private float _timer;
    private bool _onCooldown = false;

    void Start()
    {
        _timer = UnityEngine.Random.Range(3f, 16f);
        On.HeroController.TakeDamage += HeroController_TakeDamage;
    }

    void FixedUpdate()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            if (!_onCooldown)
            {
                On.HeroController.TakeDamage -= HeroController_TakeDamage;
                _timer = UnityEngine.Random.Range(5f, 11f);
                _onCooldown = true;
            }
            else
                Destroy(this);
        }
    }

    private void HeroController_TakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
    {
        if (damageAmount != 500 && damageAmount != 0)
        {
            WeakendEffect effect = go.GetComponent<WeakendEffect>() ?? go.transform.parent?.GetComponent<WeakendEffect>();
            if (effect == this)
                damageAmount = Math.Max(1, damageAmount - UnityEngine.Random.Range(1, Math.Max(2, 1 + CombatController.EnduranceLevel / 4)));
        }
        orig(self, go, damageSide, damageAmount, hazardType);
    }
}
