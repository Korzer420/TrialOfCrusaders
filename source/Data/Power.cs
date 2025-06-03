using KorzUtils.Helper;
using System;
using System.Collections;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.UnityComponents.Debuffs;
using UnityEngine;

namespace TrialOfCrusaders.Data;

public abstract class Power : IEquatable<Power>
{
    #region Members

    private bool _enabled;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the rarity of the power.
    /// <para/>Default is <see cref="Rarity.Common"/>.
    /// </summary>
    public virtual Rarity Tier => Rarity.Common;

    /// <summary>
    /// Gets the rates for bonus stats on this power.
    /// </summary>
    public virtual (float, float, float) BonusRates => new(10, 10, 10);

    /// <summary>
    /// Gets whether this power can appear in a selection.
    /// </summary>
    public virtual bool CanAppear => true;

    /// <summary>
    /// Gets the name of the power.
    /// </summary>
    public virtual string Name
    {
        get
        {
            string name = string.Empty;
            foreach (var letter in GetType().Name)
            {
                if (char.IsUpper(letter) && !string.IsNullOrEmpty(name))
                    name += $" {letter}";
                else
                    name += letter;
            }
            return name;
        }
    }

    /// <summary>
    /// Gets the description of the power.
    /// </summary>
    public virtual string Description
    {
        get
        {
            string description = Resources.Text.PowerDescriptions.ResourceManager.GetString(GetType().Name);
            // Apply debuff colors
            description = description.Replace(" bleed", $"<color={BleedEffect.TextColor}> bleed</color>");
            description = description.Replace(" concussion", $"<color={ConcussionEffect.TextColor}> concussion</color>");
            description = description.Replace(" burn", $"<color={BurnEffect.TextColor}> burn</color>");
            description = description.Replace(" weakenend", $"<color={WeakenedEffect.TextColor}> weakenend</color>");
            description = description.Replace(" root", $"<color={RootEffect.TextColor}> root</color>");
            description = description.Replace(" shattered mind", $"<color={ShatteredMindEffect.TextColor}> shattered mind</color>");
            return description;
        }
    }

    public virtual Sprite Sprite => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities.Placeholder"); //SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + GetType().Name);

    #endregion

    #region Methods

    internal void EnablePower()
    {
        if (_enabled)
        {
            LogHelper.Write("Power " + Name + " is already enabled.", KorzUtils.Enums.LogType.Warning);
            return;
        }
        Enable();
        _enabled = true;
    }

    internal void DisablePower()
    {
        if (!_enabled)
        {
            LogHelper.Write("Power " + Name + " is already disabled.", KorzUtils.Enums.LogType.Warning);
            return;
        }
        Disable();
        _enabled = false;
    }

    /// <summary>
    /// Enables the power.
    /// </summary>
    protected virtual void Enable() { }

    /// <summary>
    /// Disables the power and resets all flags.
    /// </summary>
    protected virtual void Disable() { }

    /// <summary>
    /// Starts a coroutine on the mod dummy object.
    /// </summary>
    protected Coroutine StartRoutine(IEnumerator coroutine) => TrialOfCrusaders.Holder.StartCoroutine(coroutine);

    /// <summary>
    /// Stops the coroutine on the mod dummy object.
    /// </summary>
    protected void StopRoutine(Coroutine coroutine) => TrialOfCrusaders.Holder.StopCoroutine(coroutine);

    protected bool HasPower<T>() where T : Power => CombatController.HasPower<T>(out _);

    #endregion

    #region Interface

    /// <inheritdoc/>
    public bool Equals(Power other)
    {
        if (other == null)
            return false;
        return GetType() == other.GetType();
    }

    #endregion
}
