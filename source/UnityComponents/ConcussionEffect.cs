using UnityEngine;

namespace TrialOfCrusaders.UnityComponents;

public class ConcussionEffect : MonoBehaviour
{
    #region Members

    private float _passedTime;

    private BoxCollider2D _parentCollider;

    #endregion

    public static GameObject ConcussionObject { get; set; }

    private void Start()
    {
        Destroy(GetComponent<PlayMakerFSM>());
        Destroy(GetComponent<TinkEffect>());
        Destroy(GetComponent<DamageHero>());
        transform.localScale = new(.5f, .5f);
        _parentCollider = transform.parent.GetComponent<BoxCollider2D>();
        On.HealthManager.TakeDamage += HealthManager_TakeDamage;
    }

    private void Update()
    {
        // Adjust the position
        transform.localPosition = new(0, 0 + _parentCollider.size.y / 2);
        _passedTime += Time.deltaTime;
        if (_passedTime >= ConcussiveTime)
            GameObject.Destroy(this.gameObject);
    }

    void OnDestroy()
    {
        On.HealthManager.TakeDamage -= HealthManager_TakeDamage;
    }

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        if (hitInstance.AttackType == AttackTypes.Nail && self.transform == transform.parent)
        {
            hitInstance.DamageDealt = Mathf.FloorToInt(hitInstance.DamageDealt * 1.25f);
            hitInstance.MagnitudeMultiplier *= 1.2f;
        }
        orig(self, hitInstance);
    }

    /// <summary>
    /// Gets or sets the time which has to pass, before an enemy recovers the concussion. Can be extendend by nail hits.
    /// </summary>
    public float ConcussiveTime { get; set; } = 3f;
}