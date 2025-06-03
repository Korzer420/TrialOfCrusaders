using System.Reflection;
using UnityEngine;

namespace TrialOfCrusaders.UnityComponents.CombatElements;

internal class EnemyHitbox : MonoBehaviour
{
    private static readonly MethodInfo _hitMethod = typeof(HealthManager).GetMethod("TakeDamage", BindingFlags.Instance | BindingFlags.NonPublic);
    private Collider2D _ownCollider;

    public HitInstance Hit { get; set; }

    void Start()
    {
        _ownCollider = GetComponent<Collider2D>();
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.GetComponent<HealthManager>() is not HealthManager manager)
        {
            if (_ownCollider == null)
                _ownCollider = GetComponent<Collider2D>();
            Physics2D.IgnoreCollision(_ownCollider, col);
            return;
        }
        if (!manager.IsInvincible && !manager.isDead && manager.GetComponent<EnemyShield>() == null)
        {
            _hitMethod.Invoke(manager, [Hit]);
            manager.gameObject.AddComponent<EnemyShield>()
                .SetTimer(0.25f)
                .SetType(1);
        }
    }
}
