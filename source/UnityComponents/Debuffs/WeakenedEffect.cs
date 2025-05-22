using UnityEngine;

namespace TrialOfCrusaders.UnityComponents.Debuffs;

internal class WeakenedEffect : MonoBehaviour
{
    private float _passedTime = 0f;

    public const string TextColor = "#02e9ed";

    public float Timer { get; set; }

    void Start()
    {
        if (Timer == 0f)
            Timer = Random.Range(3f, 16f);
    }

    void FixedUpdate()
    {
        _passedTime += Time.deltaTime;
        if (_passedTime >= Timer)
            Destroy(this);
    }
}
