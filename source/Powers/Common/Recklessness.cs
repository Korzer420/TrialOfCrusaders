﻿using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Common;

internal class Recklessness : Power
{
    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    public override bool CanAppear => CombatController.HasPower<GreatSlash>(out _) || CombatController.HasPower<DashSlash>(out _);

    public override DraftPool Pools => DraftPool.Combat | DraftPool.Risk | DraftPool.Upgrade;
}
