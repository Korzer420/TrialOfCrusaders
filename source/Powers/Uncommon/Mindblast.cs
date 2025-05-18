using Modding.Utils;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.UnityComponents;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class Mindblast : Power
{
    public override string Name => "Mindblast";

    public override string Description => "Dreamnail permanently causes enemies to take increased damage.";

    public override (float, float, float) BonusRates => new(20f, 20f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    internal override void Enable() => On.EnemyDreamnailReaction.RecieveDreamImpact += Apply_Mindblast;

    internal override void Disable() => On.EnemyDreamnailReaction.RecieveDreamImpact -= Apply_Mindblast;

    private void Apply_Mindblast(On.EnemyDreamnailReaction.orig_RecieveDreamImpact orig, EnemyDreamnailReaction self)
    {
        orig(self);
        MindblastEffect mindBlast = self.gameObject.GetOrAddComponent<MindblastEffect>();
        mindBlast.ExtraDamage += 10 + CombatController.SpiritLevel * 5;
    }
}
