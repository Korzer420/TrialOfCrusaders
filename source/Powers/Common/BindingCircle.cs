using Modding.Utils;
using TrialOfCrusaders.UnityComponents;

namespace TrialOfCrusaders.Powers.Common;

internal class BindingCircle : Power
{
    public override string Name => "Binding Circle";

    public override string Description => "Dreamnailing an enemy can root them shortly.";

    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    internal override void Enable()
    {
        On.EnemyDreamnailReaction.RecieveDreamImpact += EnemyDreamnailReaction_RecieveDreamImpact;
    }

    internal override void Disable()
    {
        On.EnemyDreamnailReaction.RecieveDreamImpact -= EnemyDreamnailReaction_RecieveDreamImpact;
    }

    private void EnemyDreamnailReaction_RecieveDreamImpact(On.EnemyDreamnailReaction.orig_RecieveDreamImpact orig, EnemyDreamnailReaction self)
    {
        orig(self);
        self.gameObject.GetOrAddComponent<RootEffect>();
    }
}