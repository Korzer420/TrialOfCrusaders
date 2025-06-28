using UnityEngine;

namespace TrialOfCrusaders.UnityComponents.CombatElements;

/// <summary>
/// Can be used to check for immunity of effects.
/// </summary>
internal class EnemyShield : MonoBehaviour
{
    public float Timer { get; private set; }

    void FixedUpdate()
    {
        Timer -= Time.deltaTime;
        if (Timer <= 0)
            Destroy(this);
    }

    internal EnemyShield SetTimer(float timer)
    {
        Timer = timer;
        return this;
    }
}
