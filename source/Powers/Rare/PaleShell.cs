using Modding;
using System.Collections;
using System.Reflection;
using TrialOfCrusaders.Enums;
using UnityEngine.SceneManagement;

namespace TrialOfCrusaders.Powers.Rare;

internal class PaleShell : Power
{
    public bool Shielded { get; set; }
    
    public override string Name => "Pale Shell";

    public override string Description => "The first hit in each room is ignored. Doesn't block insta death effects.";

    public override (float, float, float) BonusRates => new(0f, 0f, 100f);

    public override Rarity Tier => Rarity.Rare;

    protected override void Enable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    protected override void Disable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1) 
        => Shielded = true;

}
