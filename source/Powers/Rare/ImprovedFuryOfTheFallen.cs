using HutongGames.PlayMaker.Actions;
using KorzUtils.Helper;
using MonoMod.Cil;
using System;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Uncommon;

namespace TrialOfCrusaders.Powers.Rare;

internal class ImprovedFuryOfTheFallen : Power
{
    public override (float, float, float) BonusRates => new(100f, 0f, 0f);

    public override Rarity Tier => Rarity.Rare;

    public override bool CanAppear => PowerRef.HasPower<FuryOfTheFallen>(out _);

    public override DraftPool Pools => DraftPool.Risk | DraftPool.Combat | DraftPool.Upgrade | DraftPool.Charm;

    public bool FuryActive => PDHelper.Health <= Math.Max(1, Math.Ceiling((float)PDHelper.MaxHealth / 4));

    protected override void Enable()
    {
        IL.HeroController.Attack += HeroController_Attack;
        On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += OnIntCompareAction;
    }

    protected override void Disable()
    {
        IL.HeroController.Attack -= HeroController_Attack;
        On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter -= OnIntCompareAction;
    }

    private void OnIntCompareAction(On.HutongGames.PlayMaker.Actions.IntCompare.orig_OnEnter orig, IntCompare self)
    {
        if (string.Equals(self.Fsm.GameObjectName, "Charm Effects") && string.Equals(self.Fsm.Name, "Fury") && (string.Equals(self.State.Name, "Check HP") || string.Equals(self.State.Name, "Recheck")) && FuryActive)
        {
            if (string.Equals(self.State.Name, "Check HP"))
                self.Fsm.FsmComponent.SendEvent("FURY");
            else if (string.Equals(self.State.Name, "Recheck"))
                self.Fsm.FsmComponent.SendEvent("RETURN");
        }
        orig(self);
    }

    private void HeroController_Attack(ILContext il)
    {
        ILCursor cursor = new(il);
        // Modifies right/left slash, up slash and down slash (the first health check is for full health, which we ignore here)
        for (int i = 0; i < 3; i++)
        {
            cursor.GotoNext(MoveType.After,
            x => x.MatchLdfld<HeroController>("playerData"),
            x => x.MatchLdstr("health"),
            x => x.MatchCallvirt<PlayerData>("GetInt"));

            cursor.GotoNext(MoveType.After,
            x => x.MatchLdfld<HeroController>("playerData"),
            x => x.MatchLdstr("health"),
            x => x.MatchCallvirt<PlayerData>("GetInt"));

            cursor.EmitDelegate<Func<int, int>>((x) => FuryActive ? 1 : x);
        }
    }
}
