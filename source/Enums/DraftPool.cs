using System;

namespace TrialOfCrusaders.Enums;

/// <summary>
/// Defines the category pool a power is in.
/// <para/>This is a flag enum.
/// </summary>
[Flags]
public enum DraftPool
{
    /// <summary>
    /// Not defined.
    /// </summary>
    None = 0,

    /// <summary>
    /// Related to the nail.
    /// </summary>
    Combat = 1,

    /// <summary>
    /// Related to soul or spells.
    /// </summary>
    Spirit = 2,

    /// <summary>
    /// Related to damage reduction, masks and survivabilty.
    /// </summary>
    Endurance = 4,

    /// <summary>
    /// Related to geo or other currencies.
    /// </summary>
    Wealth = 8,

    /// <summary>
    /// Related to effects that apply immediately upon picking it up.
    /// </summary>
    Instant = 16,

    /// <summary>
    /// Related to powers which grant a huge effect, but a have big price.
    /// </summary>
    Risk = 32,

    /// <summary>
    /// Related to treasure rooms or power/ability orbs in general.
    /// </summary>
    Treasure = 64,

    /// <summary>
    /// Related to an usable ability.
    /// </summary>
    Ability = 128,

    /// <summary>
    /// Related to a charm.
    /// </summary>
    Charm = 256,

    /// <summary>
    /// Related to an upgrade of another ability or power.
    /// </summary>
    Upgrade = 512,

    /// <summary>
    /// Related to an ability which works in short intervals.
    /// </summary>
    Burst = 1024,

    /// <summary>
    /// Related to debuffs.
    /// </summary>
    Debuff = 2048
}
