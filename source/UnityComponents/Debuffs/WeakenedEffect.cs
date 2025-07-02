using UnityEngine;

namespace TrialOfCrusaders.UnityComponents.Debuffs;

internal class WeakenedEffect : MonoBehaviour
{
    private float _passedTime = 0f;
    private GameObject _effectObject;

    public static GameObject Prefab { get; set; }

    public const string TextColor = "#02e9ed";

    public float Timer { get; set; } = 5f;

    void Start()
    {
        if (Timer == 0f)
            Timer = Random.Range(3f, 16f);
        _effectObject = GameObject.Instantiate(Prefab, transform);
        _effectObject.transform.localPosition = Vector3.zero - new Vector3(0f,0f, 0.01f);
        _effectObject.GetComponent<Animator>().StartPlayback();
        _effectObject.SetActive(true);
    }

    void FixedUpdate()
    {
        _passedTime += Time.deltaTime;
        if (_passedTime >= Timer)
        {
            GameObject.Destroy(_effectObject);
            Destroy(this);
        }
    }

    internal static void PreparePrefab(GameObject original)
    {
        GameObject prefab = GameObject.Instantiate(original);
        foreach (Transform child in prefab.transform)
            GameObject.Destroy(child.gameObject);
        Component.Destroy(prefab.GetComponent<BoxCollider2D>());
        Component.Destroy(prefab.GetComponent<PlayMakerFSM>());
        Component.Destroy(prefab.LocateMyFSM("FSM"));
        Component.Destroy(prefab.GetComponent<AudioSource>());
        Component.Destroy(prefab.GetComponent<PlayMakerFixedUpdate>());
        Component.Destroy(prefab.GetComponent<PlayMakerUnity2DProxy>());
        prefab.name = "Weakened Effect";
        Prefab = prefab;
        GameObject.DontDestroyOnLoad(Prefab);
    }
}
