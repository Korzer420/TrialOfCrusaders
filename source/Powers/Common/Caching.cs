using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class Caching : Power
{
    public GameObject ActiveSoulCache { get; set; }

    public static GameObject SoulCache { get; set; }

    public override string Name => "Caching";

    public override string Description => "Excessive soul may manifest in a soul sphere. Only one sphere can be active at a time.";

    public override (float, float, float) BonusRates => new(0f, 10f, 0f);
}