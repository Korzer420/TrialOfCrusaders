using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class SoulEater : Power
{
    public override string Name => "Soul Eater";

    public override string Description => "Increases soul gain by hitting enemies significantly. Overwrites Soul Catcher.";

    public override Rarity Tier => Rarity.Uncommon;

    public override (float, float, float) BonusRates => new(0f, 40, 0f);

    public override bool CanAppear => false;

    protected override void Enable() 
    { 
        CharmHelper.EnsureEquipCharm(CharmRef.SoulEater); 
        CharmHelper.UnequipCharm(CharmRef.SoulCatcher);
    }

    protected override void Disable()
    {
        CharmHelper.UnequipCharm(CharmRef.SoulEater);
        // TODO: Check for Soul Catcher
    }
}
