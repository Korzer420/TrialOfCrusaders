using System.Collections;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class CheatDeath : Power
{
    private int _cooldown = 0;

    public override (float, float, float) BonusRates => new(0f, 0f, 40f);

    public override Rarity Tier => Rarity.Uncommon;

    public override bool CanAppear => !HasPower<InUtterDarkness>();

    protected override void Enable()
    {
        On.HeroController.Die += HeroController_Die;
        StageController.RoomEnded += StageController_RoomCleared;
    }

    private void StageController_RoomCleared(bool quietRoom) => _cooldown = _cooldown.Lower(quietRoom ? 0 : 1);

    protected override void Disable()
    {
        On.HeroController.Die -= HeroController_Die;
        StageController.RoomEnded -= StageController_RoomCleared;
    }

    private IEnumerator HeroController_Die(On.HeroController.orig_Die orig, HeroController self)
    {
        if (_cooldown != 0 || RngProvider.GetRandom(1, 21) >= CombatController.EnduranceLevel + 1)
            yield return orig(self);
        else
        {
            _cooldown = 10;
            // This only restores 2 without endurance points. I have no clue.
            HeroController.instance.AddHealth(3 + CombatController.EnduranceLevel / 2);
            HeroController.instance.StartCoroutine(UpdateUI());
        }
    }

    /// <summary>
    /// Wait a moment and then update the ui to display the health correctly.
    /// </summary>
    private IEnumerator UpdateUI()
    {
        yield return new WaitForSeconds(.2f);
        HeroController.instance.proxyFSM.SendEvent("HeroCtrl-Healed");
    }
}
