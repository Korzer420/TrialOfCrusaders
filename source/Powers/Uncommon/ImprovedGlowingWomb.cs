using HutongGames.PlayMaker;
using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ImprovedGlowingWomb : Power
{
    private FsmVariables _hatchlingVariables;

    public override (float, float, float) BonusRates => new(10f, 30f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public override DraftPool Pools => DraftPool.Combat | DraftPool.Upgrade | DraftPool.Charm;

    public FsmVariables HatchlingVariables => _hatchlingVariables ??= GameObject.Find("Charm Effects").LocateMyFSM("Hatchling Spawn").FsmVariables;

    public override bool CanAppear => HasPower<GlowingWomb>();

    public override StatScaling Scaling => StatScaling.Combat | StatScaling.Spirit;

    protected override void Enable()
    {
        CharmHelper.EnsureEquipCharm(KorzUtils.Enums.CharmRef.GlowingWomb);
        On.KnightHatchling.OnEnable += HatchlingSpawn;
        HatchlingVariables.FindFsmInt("Hatchling Max").Value = 10;
        HatchlingVariables.FindFsmInt("Soul Cost").Value = 2;
        HatchlingVariables.FindFsmFloat("Hatch Time").Value = 3f;
    }

    protected override void Disable()
    {
        HatchlingVariables.FindFsmInt("Hatchling Max").Value = 4;
        HatchlingVariables.FindFsmInt("Soul Cost").Value = 8;
        HatchlingVariables.FindFsmFloat("Hatch Time").Value = 4f;
        On.KnightHatchling.OnEnable -= HatchlingSpawn;
    }

    private void HatchlingSpawn(On.KnightHatchling.orig_OnEnable orig, KnightHatchling self)
    {
        orig(self);
        self.normalDetails.damage = 25 + CombatRef.SpiritLevel * 2 + CombatRef.CombatLevel;
        self.dungDetails.damage = 40 + CombatRef.SpiritLevel * 2 + CombatRef.CombatLevel;
    }
}