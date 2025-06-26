using System;
using TrialOfCrusaders.Enums;
using UnityEngine;

namespace TrialOfCrusaders.UnityComponents.Other;

internal class LeftShinyFlag : MonoBehaviour
{
    public static event Action<TreasureType> LeftShinyBehind;

    public TreasureType Treasure { get; set; }

    public bool Ignore { get; set; }

    void OnDestroy()
    {
        if (!Ignore)
            LeftShinyBehind?.Invoke(Treasure);
    }

    internal void RemoveFlag()
    {
        Ignore = true;
        Destroy(this);
    }
}
