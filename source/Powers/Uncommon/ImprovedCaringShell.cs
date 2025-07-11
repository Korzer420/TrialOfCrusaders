﻿using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ImprovedCaringShell : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 40f);

    public override Rarity Tier => Rarity.Uncommon;

    public override DraftPool Pools => DraftPool.Endurance | DraftPool.Upgrade;

    public override bool CanAppear => HasPower<CaringShell>();

    public override StatScaling Scaling => StatScaling.Endurance;
}
