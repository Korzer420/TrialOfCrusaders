using System;
using UnityEngine;

namespace TrialOfCrusaders.UnityComponents.PowerElements;

internal class SoulCache : MonoBehaviour
{
    private float _graceTime = 2f;

    public int SoulAmount { get; set; }

    void Update() => _graceTime = Math.Max(0, _graceTime - Time.deltaTime);

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_graceTime > 0)
            return;
        if (other.gameObject.layer == 20)
        {
            HeroController.instance.AddMPCharge(SoulAmount);
            Destroy(gameObject);
        }
        else
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), other);
    }
}
