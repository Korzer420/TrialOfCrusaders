using UnityEngine;

namespace TrialOfCrusaders.UnityComponents.CombatElements;

/// <summary>
/// Dummy behaviour to flag enemies that are present at the start of the room.
/// </summary>
internal class BaseEnemy : MonoBehaviour
{
    public bool NoLoot { get; set; }
}
