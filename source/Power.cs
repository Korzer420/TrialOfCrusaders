using System;
using System.Collections;
using System.Collections.Generic;
using TrialOfCrusaders.Enums;
using UnityEngine;

namespace TrialOfCrusaders;

public abstract class Power : IEquatable<Power>
{
    #region Properties

    public virtual Rarity Tier => Rarity.Common;

    public virtual (float, float, float) BonusRates => new(10, 10, 10);

    public abstract string Name { get; }

    public abstract string Description { get; }

    #endregion

    #region Methods

    internal virtual void Enable()
    {

    }

    internal virtual void Disable()
    {

    }

    public virtual bool CanAppear() => true;

    internal Coroutine StartRoutine(IEnumerator coroutine) => CombatController.ExecuteRoutine(coroutine);

    internal void StopRoutine(Coroutine coroutine) => CombatController.StopRoutine(coroutine);

    public bool Equals(Power x, Power y)
    {
        if (x is not null && y is null)
            return false;
        if (y is not null && x is null)
            return false;
        if (x is null)
            return true;
        return x.GetType() == y.GetType();
    }

    public int GetHashCode(Power obj) => obj.GetType().GetHashCode();

    public bool Equals(Power other)
    {
        if (other == null)
            return false;
        return GetType() == other.GetType();
    }

    #endregion
}
