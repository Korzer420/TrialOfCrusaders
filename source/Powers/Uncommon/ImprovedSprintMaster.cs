using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ImprovedSprintMaster : Power
{
    public override string Name => "Improved Sprint Master";

    public override string Description => "Grants a significant speed boost.";

    public override (float, float, float) BonusRates => new(20f, 0f, 20f);

    public override Rarity Tier => Rarity.Uncommon;

    protected override void Enable()
    {
        CharmHelper.EnsureEquipCharm(KorzUtils.Enums.CharmRef.Sprintmaster);
        
    }

    protected override void Disable()
    {
        HeroController.instance.WALK_SPEED -= 3f;
        HeroController.instance.RUN_SPEED -= 3f;
        HeroController.instance.RUN_SPEED_CH -= 3f;
        HeroController.instance.RUN_SPEED_CH_COMBO -= 3f;
        HeroController.instance.JUMP_SPEED -= 1f;
    }
}
