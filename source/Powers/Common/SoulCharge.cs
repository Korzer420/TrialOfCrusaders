using KorzUtils.Helper;
using Modding.Utils;
using System.Collections;
using TrialOfCrusaders.UnityComponents;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class SoulCharge : Power
{
    private int _spendSoul = 0;
    public int SpendThreshold => 160 - CombatController.SpiritLevel * 2;

    public static GameObject Orb { get; set; }

    public override string Name => "Soul Charge";

    public override string Description => "Spending soul has a chance to spawn an temporary rotating orb at your location that deals damage.";

    public override (float, float, float) BonusRates => new(0f, 10f, 0f);

    protected override void Enable()
    {
        On.PlayerData.TakeMP += PlayerData_TakeMP;
        _spendSoul = 0;
    }

    protected override void Disable() => On.PlayerData.TakeMP -= PlayerData_TakeMP;

    private void PlayerData_TakeMP(On.PlayerData.orig_TakeMP orig, PlayerData self, int amount)
    {
        orig(self, amount);
        _spendSoul += amount;
        if (_spendSoul >= SpendThreshold)
        {
            _spendSoul -= SpendThreshold;
            SpawnOrb();
        }
    }

    private void SpawnOrb()
    {
        GameObject orbParent = new("Orb Container");
        orbParent.transform.position = HeroController.instance.transform.position;
        orbParent.SetActive(true);
        GameObject orb = GameObject.Instantiate(Orb, orbParent.transform);
        orb.transform.localScale = new(1.25f, 1.25f, 1f);
        orb.transform.localPosition = new(0f, 3f);
        Component.Destroy(orb.LocateMyFSM("Orb Control"));
        // Prevent the reappearing of the damage hero hitbox.
        Component.Destroy(orb.GetComponent<PlayMakerFixedUpdate>());
        Component.Destroy(orb.GetComponent<AudioSource>());
        Component.Destroy(orb.GetComponent<Rigidbody2D>());
        Component.Destroy(orb.GetComponent<CircleCollider2D>());
        GameObject.Destroy(orb.transform.Find("Hero Hurter").gameObject);
        BoxCollider2D collider = orb.AddComponent<BoxCollider2D>();
        collider.size = new(3f, 3f);
        collider.isTrigger = true;
        orb.AddComponent<EnemyHitbox>().Hit = new()
        {
            AttackType = AttackTypes.Spell,
            MagnitudeMultiplier = 0f,
            Multiplier = 1f,
            DamageDealt = CombatController.SpiritLevel * 3
        };
        orb.layer = 17;
        orb.SetActive(true);
        orbParent.AddComponent<Dummy>().StartCoroutine(OrbRotation(orbParent));
    }

    private IEnumerator OrbRotation(GameObject orb)
    {
        float passedTime = 0f;
        float rotation = 0f;
        while(passedTime < 15f)
        {
            rotation += 1;
            if (rotation >= 360)
                rotation = 0;
            orb.transform.SetRotation2D(rotation);
            passedTime += Time.deltaTime;
            yield return null;
        }
        GameObject.Destroy(orb);
    }
}
