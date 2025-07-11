﻿using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class Caching : Power
{
    public override DraftPool Pools => DraftPool.Spirit;

    public GameObject ActiveSoulCache { get; set; }

    public static GameObject SoulCache { get; set; }

    public override (float, float, float) BonusRates => new(0f, 10f, 0f);

    public override StatScaling Scaling => StatScaling.Spirit;
}