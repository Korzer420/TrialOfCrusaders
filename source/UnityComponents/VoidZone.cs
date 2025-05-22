using KorzUtils.Helper;
using Modding.Utils;
using System.Reflection;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.UnityComponents.Debuffs;
using UnityEngine;

namespace TrialOfCrusaders.UnityComponents;

internal class VoidZone : MonoBehaviour
{
    public static GameObject Ring { get; set; }

    private MethodInfo _takeDamage = typeof(HealthManager).GetMethod("TakeDamage", BindingFlags.NonPublic | BindingFlags.Instance);

    public float LeftTime { get; set; } = 3f;

    void Start()
    {
        var collider = gameObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        gameObject.layer = 17;
        collider.radius = 3.5f;
        GetComponent<SpriteRenderer>().color = new(1f, 0f, 1f);
    }

    void Update()
    {
        LeftTime -= Time.deltaTime;
        if (LeftTime <= 0)
            GameObject.Destroy(transform.parent.gameObject);
        transform.localScale += new Vector3(Time.deltaTime * 2.5f, Time.deltaTime * 2.5f);
        if (transform.localScale.x > 3f)
            transform.localScale = new Vector3(0f, 0f);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer == 11) // Enemy layer
        {
            if (other.GetComponent<EnemyShield>() is not EnemyShield shield || shield.Type != 4)
            {
                HealthManager enemy = other.gameObject.GetComponent<HealthManager>();
                if (enemy != null)
                {
                    _takeDamage.Invoke(enemy, [new HitInstance()
                    {
                        AttackType = AttackTypes.SharpShadow,
                        Source = gameObject,
                        DamageDealt = 5 + CombatController.CombatLevel + CombatController.SpiritLevel + CombatController.EnduranceLevel,
                        Multiplier = 1f
                    }]);
                    if (UnityEngine.Random.Range(0, 10) < 1)
                    {
                        int rolled = UnityEngine.Random.Range(0, 6);
                        switch (rolled)
                        {
                            // Weakened
                            case 0:
                                enemy.gameObject.GetOrAddComponent<WeakenedEffect>().Timer += 3f;
                                break;
                            // Concussion
                            case 1:
                                enemy.gameObject.GetOrAddComponent<ConcussionEffect>().Timer += 3f;
                                break;
                            // Burn
                            case 2:
                                enemy.gameObject.GetOrAddComponent<BurnEffect>().AddDamage(CombatController.SpiritLevel);
                                break;
                            // Bleed
                            case 3:
                                enemy.gameObject.AddComponent<BleedEffect>();
                                break;
                            // Root
                            case 4:
                                enemy.gameObject.GetOrAddComponent<RootEffect>();
                                break;
                            // Shattered mind
                            default:
                                enemy.gameObject.GetOrAddComponent<ShatteredMindEffect>().ExtraDamage += CombatController.SpiritLevel / 5;
                                break;
                        }
                    }
                }
                other.gameObject.AddComponent<EnemyShield>().SetTimer(0.2f);
                other.gameObject.GetComponent<EnemyShield>().SetType(4);
            }
        }
    }
}
