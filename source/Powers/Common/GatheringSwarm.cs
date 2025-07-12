using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class GatheringSwarm : Power
{
    public override bool CanAppear => !CombatRef.HasPower<ShiningBound>(out _);

    public override (float, float, float) BonusRates => new(0f, 0, 0f);

    public override DraftPool Pools => DraftPool.Charm | DraftPool.Wealth;

    public override Sprite Sprite => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + GetType().Name);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.GatheringSwarm);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.GatheringSwarm);
}
