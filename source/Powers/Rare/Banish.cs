using System.Collections;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Rare;

internal class Banish : Power
{
    private Coroutine _listener;

    private int _leftover = 0;

    #region Power Data
    
    public override Rarity Tier => Rarity.Rare;

    public override bool CanAppear => !HasPower<DreamPortal>();

    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override DraftPool Pools => DraftPool.Retain | DraftPool.Ability;

    #endregion

    public bool CanInput => HeroController.instance?.acceptingInput == true && GameManager.instance?.IsGamePaused() == false;

    protected override void Enable()
    {
        _leftover = 3;
        if (_listener != null)
            StopRoutine(_listener);
        _listener = StartRoutine(WaitForInput());
    }

    protected override void Disable()
    {
        if (_listener != null)
            StopRoutine(_listener);
    }

    private IEnumerator WaitForInput()
    {
        while (true)
        {
            yield return null;
        }
    }
}
