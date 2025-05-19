namespace TrialOfCrusaders.Powers.Common;

internal class BrutalStrikes : Power
{
    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    public override bool CanAppear => CombatController.HasNailArt();
}