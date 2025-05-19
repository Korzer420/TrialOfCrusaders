using KorzUtils.Helper;
using Modding.Utils;
using TrialOfCrusaders.UnityComponents;

namespace TrialOfCrusaders.Powers.Common;

internal class BindingCircle : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    public override bool CanAppear => PDHelper.HasDreamNail;

    protected override void Enable() => On.EnemyDreamnailReaction.RecieveDreamImpact += EnemyDreamnailReaction_RecieveDreamImpact;

    protected override void Disable() => On.EnemyDreamnailReaction.RecieveDreamImpact -= EnemyDreamnailReaction_RecieveDreamImpact;

    private void EnemyDreamnailReaction_RecieveDreamImpact(On.EnemyDreamnailReaction.orig_RecieveDreamImpact orig, EnemyDreamnailReaction self)
    {
        self.gameObject.GetOrAddComponent<RootEffect>();
        orig(self);
    }
}