using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Common;

internal class BrutalStrikes : Power
{
    public override DraftPool Pools => DraftPool.Combat | DraftPool.Upgrade;

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    public override bool CanAppear => PowerRef.HasNailArt();
}