using UnityEngine;

namespace TrialOfCrusaders.UnityComponents;

internal class EnemyShield : MonoBehaviour
{
    public float Timer { get; private set; }

    public int Type { get; private set; }

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

    internal EnemyShield SetType(int type)
    {
        Type = type;
        return this;
    }
}
