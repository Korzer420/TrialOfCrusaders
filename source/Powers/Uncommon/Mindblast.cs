using Modding.Utils;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.UnityComponents.Debuffs;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class Mindblast : Power
{
    public override (float, float, float) BonusRates => new(20f, 20f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public override bool CanAppear => HasPower<DreamNail>();

    public override StatScaling Scaling => StatScaling.Spirit;

    protected override void Enable() => On.EnemyDreamnailReaction.RecieveDreamImpact += Apply_Mindblast;

    protected override void Disable() => On.EnemyDreamnailReaction.RecieveDreamImpact -= Apply_Mindblast;

    private void Apply_Mindblast(On.EnemyDreamnailReaction.orig_RecieveDreamImpact orig, EnemyDreamnailReaction self)
    {
        orig(self);
        ShatteredMindEffect mindBlast = self.gameObject.GetOrAddComponent<ShatteredMindEffect>();
        mindBlast.ExtraDamage += 20 + CombatController.SpiritLevel * 5;
    }
}
