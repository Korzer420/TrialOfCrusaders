using TrialOfCrusaders.Data;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class Caching : Power
{
    public GameObject ActiveSoulCache { get; set; }

    public static GameObject SoulCache { get; set; }

    public override (float, float, float) BonusRates => new(0f, 10f, 0f);
}