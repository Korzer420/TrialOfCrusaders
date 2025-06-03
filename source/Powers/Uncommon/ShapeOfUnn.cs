using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ShapeOfUnn : Power
{
    public override (float, float, float) BonusRates => new(0f, 5f, 35f);

    public override Rarity Tier => Rarity.Uncommon;

    public override Sprite Sprite => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + GetType().Name);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.ShapeOfUnn);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.ShapeOfUnn);
}
