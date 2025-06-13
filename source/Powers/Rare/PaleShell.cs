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

    protected override void Enable() => UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

    protected override void Disable() => UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1) => Shielded = true;
}
