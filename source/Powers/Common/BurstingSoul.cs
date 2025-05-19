using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class BurstingSoul : Power
{
    public int SpellCount { get; set; } = 0;

    public override string Name => "Bursting Soul";

    public override string Description => "Spells deal increased damage, but each cast lowers spell damage for the rest of the room.";

    public override (float, float, float) BonusRates => new(0f, 10f, 0f);

    protected override void Enable()
    {
        SpellCount = 0;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        On.HutongGames.PlayMaker.Actions.SetVelocity2d.OnEnter += SetVelocity2d_OnEnter;
    }

    protected override void Disable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        On.HutongGames.PlayMaker.Actions.SetVelocity2d.OnEnter -= SetVelocity2d_OnEnter;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        => SpellCount = 0;

    private void SetVelocity2d_OnEnter(On.HutongGames.PlayMaker.Actions.SetVelocity2d.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetVelocity2d self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Spell End"))
            SpellCount++;
        orig(self);
    }
}