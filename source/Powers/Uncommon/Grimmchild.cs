using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class Grimmchild : Power
{
    public override (float, float, float) BonusRates => new(0f, 20f, 20f);

    public override Rarity Tier => Rarity.Uncommon;

    public override DraftPool Pools => DraftPool.Combat | DraftPool.Charm;

    public override bool CanAppear => !HasPower<CarefreeMelody>() && !HasPower<ShiningBound>();

    protected override void Enable()
    {
        CharmHelper.EnsureEquipCharm(CharmRef.Grimmchild3);
        if (PDHelper.GrimmChildLevel != 3)
            PDHelper.GrimmChildLevel = 3;
    }

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.Grimmchild3);
}
