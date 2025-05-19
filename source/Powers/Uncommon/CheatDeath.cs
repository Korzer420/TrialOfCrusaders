using KorzUtils.Helper;
using System.Collections;
using TrialOfCrusaders.Enums;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class CheatDeath : Power
{
    private int _cooldown = 0;

    public override string Name => "Cheat Death";

    public override string Description => "Prevents lethal hits... sometimes. Can only occur once per 10 cleared rooms. Does not work on instant kill effects.";

    public override (float, float, float) BonusRates => new(0f, 0f, 40f);

    public override Rarity Tier => Rarity.Uncommon;

    protected override void Enable()
    {
        On.HeroController.Die += HeroController_Die;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    protected override void Disable()
    {
        On.HeroController.Die -= HeroController_Die;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        => _cooldown = Mathf.Max(0, _cooldown--);

    private IEnumerator HeroController_Die(On.HeroController.orig_Die orig, HeroController self)
    {
        if (_cooldown != 0 || Random.Range(0, 21) >= CombatController.EnduranceLevel + 20)
            yield return orig(self);
        else
        {
            _cooldown = 10;
            // This only restores 2 without survival points. I have no clue.
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
