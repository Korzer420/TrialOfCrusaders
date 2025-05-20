using UnityEngine;

namespace TrialOfCrusaders.UnityComponents.Debuffs;

public class ConcussionEffect : MonoBehaviour
{
    #region Members

    private float _passedTime;

    private BoxCollider2D _collider;

    private GameObject _visualIndicator;

    #endregion

    public static GameObject ConcussionObject { get; set; }

    private void Start()
    {
        _visualIndicator = Instantiate(ConcussionObject, transform);
        Destroy(_visualIndicator.GetComponent<PlayMakerFSM>());
        Destroy(_visualIndicator.GetComponent<TinkEffect>());
        Destroy(_visualIndicator.GetComponent<DamageHero>());
        _visualIndicator.transform.localScale = new(.5f, .5f);
        _collider = gameObject.GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        // Adjust the position
        _visualIndicator.transform.localPosition = new(0, 0 + _collider.size.y / 2);
        _passedTime += Time.deltaTime;
        if (_passedTime >= Timer)
            Destroy(_visualIndicator);
    }

    /// <summary>
    /// Gets or sets the time which has to pass, before an enemy recovers the concussion. Can be extendend by nail hits.
    /// </summary>
    public float Timer { get; set; } = 3f;
}