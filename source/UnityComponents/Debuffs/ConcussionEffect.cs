using UnityEngine;

namespace TrialOfCrusaders.UnityComponents.Debuffs;

public class ConcussionEffect : MonoBehaviour
{
    #region Members

    private float _passedTime;

    private BoxCollider2D _collider;

    private GameObject _visualIndicator;

    public const string TextColor = "#8f8c8e";

    #endregion

    internal static GameObject Prefab { get; set; }

    /// <summary>
    /// Gets or sets the time which has to pass, before an enemy recovers the concussion. Can be extendend by nail hits.
    /// </summary>
    public float Timer { get; set; } = 3f;

    private void Start()
    {
        _visualIndicator = Instantiate(Prefab, transform);
        _visualIndicator.transform.localScale = new(.5f, .5f);
        _visualIndicator.SetActive(true);
        _collider = gameObject.GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        // Adjust the position
        _visualIndicator.transform.localPosition = new(0, 0 + _collider.size.y / 2);
        _passedTime += Time.deltaTime;
        if (_passedTime >= Timer)
        { 
            Destroy(_visualIndicator);
            Destroy(this);
        }
    }

    internal static void PreparePrefab(GameObject prefab)
    {
        Destroy(prefab.GetComponent<PlayMakerFSM>());
        Destroy(prefab.GetComponent<TinkEffect>());
        Destroy(prefab.GetComponent<DamageHero>());
        Destroy(prefab.GetComponent<BoxCollider2D>());
        Destroy(prefab.GetComponent<PlayMakerFixedUpdate>());
        Destroy(prefab.GetComponent<ObjectBounce>());
        prefab.GetComponent<tk2dSpriteAnimator>().playAutomatically = true;
        prefab.GetComponent<tk2dSpriteAnimator>().ClipFps /= 2;
        prefab.name = "Concussion Effect";
        Prefab = prefab;
        GameObject.DontDestroyOnLoad(prefab);
    }
}