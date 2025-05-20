using KorzUtils.Helper;
using Modding.Utils;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.UnityComponents;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class Mindblast : Power
{
    public override (float, float, float) BonusRates => new(20f, 20f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public override bool CanAppear => HasPower<DreamNail>();

    protected override void Enable() => On.EnemyDreamnailReaction.RecieveDreamImpact += Apply_Mindblast;

    protected override void Disable() => On.EnemyDreamnailReaction.RecieveDreamImpact -= Apply_Mindblast;

    private void Apply_Mindblast(On.EnemyDreamnailReaction.orig_RecieveDreamImpact orig, EnemyDreamnailReaction self)
    {
        orig(self);
        ShatteredMindEffect mindBlast = self.gameObject.GetOrAddComponent<ShatteredMindEffect>();
        mindBlast.ExtraDamage += 10 + CombatController.SpiritLevel * 5;
    }
}
