using UnityEngine;

namespace TrialOfCrusaders.UnityComponents.Debuffs;

public class ShatteredMindEffect : MonoBehaviour
{
    private GameObject _effectObject;

    public const string TextColor = "#f6ff00";

    public int ExtraDamage { get; set; }

    public static GameObject Prefab { get; set; }

    void Start()
    {
        _effectObject = GameObject.Instantiate(Prefab, transform);
        _effectObject.transform.localPosition = new Vector3(0,0,-0.01f);
        _effectObject.SetActive(true);
    }

    internal static void PreparePrefab(GameObject original)
    {
        GameObject prefab = GameObject.Instantiate(original);
        prefab.name = "Shattered Mind Effect";
        prefab.GetComponent<ParticleSystem>().enableEmission = true;
        prefab.GetComponent<ParticleSystem>().emissionRate = 10;
        prefab.GetComponent<ParticleSystem>().loop = true;
        Prefab = prefab;
    }
}
