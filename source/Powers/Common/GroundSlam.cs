using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.UnityComponents.CombatElements;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class GroundSlam : Power
{
    public override (float, float, float) BonusRates => new(8f, 0f, 2f);

    public static GameObject Shockwave { get; set; }

    protected override void Enable()
    {
        On.HeroController.DoHardLanding += HeroController_DoHardLanding;
        On.SetDamageHeroAmount.OnEnter += SetDamageHeroAmount_OnEnter;
    }

    private void SetDamageHeroAmount_OnEnter(On.SetDamageHeroAmount.orig_OnEnter orig, SetDamageHeroAmount self)
    {
        orig(self);
        if (self.IsCorrectContext("Damage timing", null, "Activate"))
        { 
            Component.Destroy(self.Fsm.GameObject.GetComponent<DamageHero>());
            self.Fsm.GameObject.AddComponent<EnemyHitbox>().Hit = new HitInstance()
            {
                Source = self.Fsm.GameObject,
                AttackType = AttackTypes.Generic,
                DamageDealt = CombatController.CombatLevel * 3,
                Multiplier = 1,
                MagnitudeMultiplier = 1.1f
            };
        }
    }

    private void HeroController_DoHardLanding(On.HeroController.orig_DoHardLanding orig, HeroController self)
    {
        orig(self);
        GameObject shockwaveLeft = GameObject.Instantiate(Shockwave);
        shockwaveLeft.name = "Shockwave left";
        shockwaveLeft.transform.localScale = new(1.25f, 1.25f);
        shockwaveLeft.transform.position = self.transform.position - new Vector3(0f, 1.4f);
        shockwaveLeft.SetActive(false);
        PlayMakerFSM shockwaveFsm = shockwaveLeft.LocateMyFSM("shockwave");
        shockwaveFsm.FsmVariables.FindFsmFloat("Speed").Value = 25f;
        shockwaveFsm.FsmVariables.FindFsmBool("Facing Right").Value = false;
        Component.Destroy(shockwaveLeft.GetComponent<DamageHero>());

        GameObject shockWaveRight = GameObject.Instantiate(Shockwave);
        shockWaveRight.name = "Shockwave right";
        shockWaveRight.transform.localScale = new(1.25f, 1.25f);
        shockWaveRight.transform.position = self.transform.position - new Vector3(0f, 1.4f);
        shockWaveRight.SetActive(false);
        shockwaveFsm = shockWaveRight.LocateMyFSM("shockwave");
        shockwaveFsm.FsmVariables.FindFsmFloat("Speed").Value = 25f;
        shockwaveFsm.FsmVariables.FindFsmBool("Facing Right").Value = true;
        Component.Destroy(shockWaveRight.GetComponent<DamageHero>());

        shockwaveLeft.SetActive(true);
        shockWaveRight.SetActive(true);
    }

    protected override void Disable()
    {
        On.HeroController.DoHardLanding -= HeroController_DoHardLanding;
        On.SetDamageHeroAmount.OnEnter -= SetDamageHeroAmount_OnEnter;
    }
}