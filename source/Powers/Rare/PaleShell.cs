using Modding;
using System.Collections;
using System.Reflection;
using TrialOfCrusaders.Enums;
using UnityEngine.SceneManagement;

namespace TrialOfCrusaders.Powers.Rare;

internal class PaleShell : Power
{
    private bool _shielded = true;

    public override string Name => "Pale Shell";

    public override string Description => "The first hit in each room is ignored. Doesn't block insta death effects.";

    public override (float, float, float) BonusRates => new(0f, 0f, 100f);

    public override Rarity Tier => Rarity.Rare;

    internal override void Enable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        ModHooks.AfterTakeDamageHook += ModHooks_AfterTakeDamageHook;
    }

    internal override void Disable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        ModHooks.AfterTakeDamageHook -= ModHooks_AfterTakeDamageHook;
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1) 
        => _shielded = true;

    private int ModHooks_AfterTakeDamageHook(int hazardType, int damageAmount)
    {
        if (_shielded && damageAmount != 500)
        {
            damageAmount = 0;
            _shielded = false;
        }
        return damageAmount;
    }
}
