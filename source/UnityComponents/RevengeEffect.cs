using UnityEngine;

namespace TrialOfCrusaders.UnityComponents;

internal class RevengeEffect : MonoBehaviour
{
    private float _passedTime = 0f;

    void FixedUpdate()
    {
        _passedTime += Time.deltaTime;
        if (_passedTime > 3f)
            Destroy(this);
    }
}
