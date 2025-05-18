using KorzUtils.Helper;
using System;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class BurstingSoul : Power
{
    private int _spellCount = 0;
    public override string Name => "Bursting Soul";

    public override string Description => "Spells deal increased damage, but each cast lowers spell damage for the rest of the room.";

    public override (float, float, float) BonusRates => new(0f, 10f, 0f);
    
    internal override void Enable()
    {
        _spellCount = 0;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter += TakeDamage_OnEnter;
        On.HutongGames.PlayMaker.Actions.SetVelocity2d.OnEnter += SetVelocity2d_OnEnter;
    }

    internal override void Disable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter -= TakeDamage_OnEnter;
        On.HutongGames.PlayMaker.Actions.SetVelocity2d.OnEnter -= SetVelocity2d_OnEnter;
    }

    private void SetVelocity2d_OnEnter(On.HutongGames.PlayMaker.Actions.SetVelocity2d.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetVelocity2d self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Spell End"))
            _spellCount++;
        orig(self);
    }

    private void TakeDamage_OnEnter(On.HutongGames.PlayMaker.Actions.TakeDamage.orig_OnEnter orig, HutongGames.PlayMaker.Actions.TakeDamage self)
    {
        int vanillaDamage = self.DamageDealt.Value;
        if (self.IsCorrectContext("damages_enemy", null, "Send Event"))
        {
            string gameObjectName = self.Fsm.GameObjectName;
            string parentName = self.Fsm.GameObject.transform.parent?.name;
            if (gameObjectName.Contains("Fireball")
                || gameObjectName == "Q Fall Damage"
                || (gameObjectName == "Hit U" && (parentName == "Scr Heads" || parentName == "Scr Heads 2"))
                || ((gameObjectName == "Hit R" || gameObjectName == "Hit L") && (parentName == "Q Slam" || parentName == "Q Slam 2" || parentName == "Q Mega" || parentName == "Scr Heads" || parentName == "Scr Heads 2")))
            { 
                self.DamageDealt = Math.Max(1, Mathf.FloorToInt(self.DamageDealt.Value * (2.2f - _spellCount * 0.2f)));
                LogHelper.Write("Spell damage: " + self.DamageDealt.Value);
            }
        }
        orig(self);
        self.DamageDealt = vanillaDamage;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        => _spellCount = 0;
}