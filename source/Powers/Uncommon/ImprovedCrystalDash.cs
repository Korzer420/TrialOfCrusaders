using KorzUtils.Helper;
using System.Collections;
using System.Reflection;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.UnityComponents;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ImprovedCrystalDash : Power
{
    private float _invincibilityCooldown = 0f;

    private MethodInfo _invulnerableCall;

    public override string Name => "Improved Crystal Dash";

    public override string Description => "Damage is increased. Grants invincibility upon start. Invinciblity has a cooldown.";

    public override (float, float, float) BonusRates => new(20f, 0f, 20f);

    public override Rarity Tier => Rarity.Uncommon;

    internal override void Enable()
    {
        On.HutongGames.PlayMaker.Actions.SetBoolValue.OnEnter += SetBoolValue_OnEnter;
        On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter += TakeDamage_OnEnter;
        _invulnerableCall = typeof(HeroController).GetMethod("Invulnerable", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    private void TakeDamage_OnEnter(On.HutongGames.PlayMaker.Actions.TakeDamage.orig_OnEnter orig, HutongGames.PlayMaker.Actions.TakeDamage self)
    {
        bool isSuperDash = self.IsCorrectContext("damages_enemy", "SuperDash Damage", "Send Event");
        int damageApplier = 20 + CombatController.CombatLevel * 5;
        if (isSuperDash)
            self.DamageDealt.Value += damageApplier;
        orig(self);
        if (isSuperDash)
            self.DamageDealt.Value -= damageApplier;
    }

    private void SetBoolValue_OnEnter(On.HutongGames.PlayMaker.Actions.SetBoolValue.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetBoolValue self)
    {
        if (_invincibilityCooldown <= 0f && self.IsCorrectContext("Superdash", "Knight", "Dash Start"))
        {
            HeroController.instance.StartCoroutine((IEnumerator)_invulnerableCall.Invoke(HeroController.instance, [3f]));
            GameObject holder = new("Cdash cooldown");
            holder.AddComponent<Dummy>().StartCoroutine(WaitInvincibilty(holder));
            GameObject.DontDestroyOnLoad(holder);
        }
        orig(self);
    }

    internal override void Disable()
    {
        On.HutongGames.PlayMaker.Actions.SetBoolValue.OnEnter -= SetBoolValue_OnEnter;
    }

    private IEnumerator WaitInvincibilty(GameObject dummy)
    {
        _invincibilityCooldown = 15f - (CombatController.EnduranceLevel / 4f);
        while(_invincibilityCooldown > 0f && HeroController.instance != null)
        {
            if (!HeroController.instance.acceptingInput)
                yield return new WaitUntil(() => HeroController.instance?.acceptingInput == true || HeroController.instance == null);
            yield return null;
            _invincibilityCooldown -= Time.deltaTime;
        }
        _invincibilityCooldown = 0f;
        GameObject.Destroy(dummy);
    }
}
