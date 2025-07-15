using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class GlowingWomb : Power
{
    public override bool CanAppear => !PowerRef.HasPower<ShiningBound>(out _);

    public override (float, float, float) BonusRates => new(2f, 8f, 0f);

    public override DraftPool Pools => DraftPool.Spirit | DraftPool.Combat | DraftPool.Charm; 

    public override Sprite Sprite => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + GetType().Name);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.GlowingWomb);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.GlowingWomb);
}
