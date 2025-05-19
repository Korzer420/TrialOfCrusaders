using UnityEngine;

namespace TrialOfCrusaders.UnityComponents;

public class ShatteredMindEffect : MonoBehaviour
{
    public int ExtraDamage { get; set; }

    void Start() => On.HealthManager.TakeDamage += HealthManager_TakeDamage;

    void OnDestroy() => On.HealthManager.TakeDamage -= HealthManager_TakeDamage;

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        if (self.gameObject == gameObject)
            hitInstance.DamageDealt += ExtraDamage;
        orig(self, hitInstance);
    }
}
