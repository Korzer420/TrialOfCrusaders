using Modding;
using System.Collections;
using TrialOfCrusaders.Enums;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Rare;

internal class HotStreak : Power
{
    #region Members

    private int _damageStacks = 0;
    private bool _currentlyRunning;
    private bool _hasHitEnemy;

    #endregion

    #region Properties
    
    public override string Name => "Hot Streak";

    public override string Description => "Increases nail damage for each consecutive nail hit. Failing to hit an enemy resets the counter.";

    public override Rarity Tier => Rarity.Rare;

    public override (float, float, float) BonusRates => new(80f, 0f, 0f); 

    #endregion

    #region Private Methods

    internal override void Enable()
    {
        ModHooks.SlashHitHook += NailSlash;
        ModHooks.GetPlayerIntHook += EmpowerNail;
    }

    internal override void Disable()
    {
        ModHooks.SlashHitHook -= NailSlash;
        ModHooks.GetPlayerIntHook -= EmpowerNail;
    }

    /// <summary>
    /// Waits for the hit to finish and then checks if an enemy was hit.
    /// </summary>
    private IEnumerator HitCooldown()
    {
        // Give the event handler time to acknowledge a hit.
        yield return new WaitForSeconds(0.25f);

        if (_hasHitEnemy)
        {
            if (_damageStacks < 5 + CombatController.CombatLevel * 2)
                _damageStacks++;
        }
        else
            _damageStacks = 0;

        UpdateNail();
    }

    /// <summary>
    /// Updates the nail and resets the flags.
    /// </summary>
    private void UpdateNail()
    {
        HeroController.instance.StartCoroutine(WaitThenUpdate());
        _hasHitEnemy = false;
        _currentlyRunning = false;
    }

    /// <summary>
    /// Wait a frame and then call upon a nail damage update.
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitThenUpdate()
    {
        yield return null;
        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
    }

    #endregion

    #region Eventhandler

    /// <summary>
    /// Event handler when the player slashes with the nail.
    /// </summary>
    private void NailSlash(Collider2D otherCollider, GameObject slash)
    {
        // This event is fired multiple times, therefore we check every instance if an enemy was hit
        if (otherCollider.gameObject.GetComponent<HealthManager>())
            _hasHitEnemy = true;
        // To prevent running multiple coroutines
        if (_currentlyRunning)
            return;
        _currentlyRunning = true;
        HeroController.instance.StartCoroutine(HitCooldown());
    }

    /// <summary>
    /// Event handler, when the game asks for the nail damage.
    /// </summary>
    private int EmpowerNail(string name, int damage)
    {
        if (string.Equals(name, "nailDamage"))
            damage += _damageStacks;
        return damage;
    }

    #endregion
}
