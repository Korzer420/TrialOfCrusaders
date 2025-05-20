using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class InUtterDarkness : Power
{
    public override (float, float, float) BonusRates => new(40f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public bool EffectGranted { get; set; }

    protected override void Enable()
    {
        if (!EffectGranted)
            HeroController.instance.MaxHealth();
        EffectGranted = true;
    }

    protected override void Disable()
    {

    }
}
