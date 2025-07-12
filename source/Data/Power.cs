using KorzUtils.Helper;
using System;
using System.Collections;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;
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

    /// <summary>
    /// Gets the stat scaling that apply to this ability.
    /// </summary>
    public virtual StatScaling Scaling { get; }

    /// <summary>
    /// Gets the name with the stat scaling attached.
    /// </summary>
    public string ScaledName
    {
        get
        {
            string name = Name + ((int)Scaling switch
            { 
                1 => " (C)",
                2 => " (S)",
                3 => " (C, S)",
                4 => " (E)",
                5 => " (C, E)",
                6 => " (S, E)",
                7 => " (C, S, E)",
                _ => ""
            });
            return name;
        }
    }

    /// <summary>
    /// Gets or sets the draft pools this power is in.
    /// </summary>
    public virtual DraftPool Pools => DraftPool.None;

    #endregion

    #region Shorthands

    protected static CombatController CombatRef => ControllerShorthands.CombatRef;

    protected static StageController StageRef => ControllerShorthands.StageRef;

    protected static ScoreController ScoreRef => ControllerShorthands.ScoreRef;

    #endregion

    #region Methods

    internal void EnablePower()
    {
        if (_enabled)
        {
            LogManager.Log("Power " + Name + " is already enabled.", KorzUtils.Enums.LogType.Warning);
            return;
        }
        try
        {
            Enable();
            _enabled = true;
            LogManager.Log("Enabled power " + Name);
            return;
        }
        catch (Exception ex)
        {
            LogManager.Log("Failed to enable power " + Name, ex);
        }
    }

    internal void DisablePower()
    {
        if (!_enabled)
        {
            LogManager.Log("Power " + Name + " is already disabled.", KorzUtils.Enums.LogType.Warning);
            return;
        }
        try
        {
            Disable();
            _enabled = false;
            LogManager.Log("Disabled power " + Name);
        }
        catch (Exception ex)
        {
            LogManager.Log("Failed to disable power " + Name, ex);
        }
        
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

    protected bool HasPower<T>() where T : Power => CombatRef.HasPower<T>(out _);

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
