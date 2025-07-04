﻿using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class CheatDeath : Power
{
    public int Cooldown { get; set; } = 0;

    public override (float, float, float) BonusRates => new(0f, 0f, 40f);

    public override Rarity Tier => Rarity.Uncommon;

    public override DraftPool Pools => DraftPool.Endurance | DraftPool.Burst;

    public override bool CanAppear => !HasPower<InUtterDarkness>();

    protected override void Enable()
    {
        Cooldown = 0;
        StageController.RoomEnded += StageController_RoomCleared;
    }

    protected override void Disable() => StageController.RoomEnded -= StageController_RoomCleared;
    
    private void StageController_RoomCleared(bool quietRoom, bool traversed) => Cooldown = Cooldown.Lower(quietRoom ? 0 : 1);
}
