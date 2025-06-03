using Modding;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Uncommon;
using UnityEngine.SceneManagement;

namespace TrialOfCrusaders.Powers.Rare;

internal class PaleShell : Power
{
    public bool Shielded { get; set; }

    public override (float, float, float) BonusRates => new(0f, 0f, 100f);

    public override Rarity Tier => Rarity.Rare;

    public override bool CanAppear => !HasPower<FragileStrength>() && !HasPower<FragileSpirit>() && !HasPower<FragileGreed>();

    protected override void Enable()
    { 
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        ModHooks.AfterTakeDamageHook += ModHooks_AfterTakeDamageHook;
    }

    protected override void Disable()
    { 
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        ModHooks.AfterTakeDamageHook -= ModHooks_AfterTakeDamageHook;
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1) => Shielded = true;
    
    private int ModHooks_AfterTakeDamageHook(int hazardType, int damageAmount)
    {
        if (Shielded)
        {
            damageAmount = 0;
            Shielded = false;
        }
        return damageAmount;
    }

}
