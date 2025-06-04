using System;

namespace TrialOfCrusaders.Enums;

/// <summary>
/// Used to represent scaling on abilities.
/// </summary>
[Flags]
public enum StatScaling
{
    /// <summary>
    /// The ability scales with the combat level.
    /// </summary>
    Combat = 1,

    /// <summary>
    /// The ability scales with the spirit level.
    /// </summary>
    Spirit = 2,

    /// <summary>
    /// The ability scales with the endurance level.
    /// </summary>
    Endurance = 4
}
