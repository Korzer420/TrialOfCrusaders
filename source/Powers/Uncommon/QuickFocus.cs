using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class QuickFocus : Power
{
    public override (float, float, float) BonusRates => new(0f, 10f, 30f);

    public override Rarity Tier => Rarity.Uncommon;

    public override Sprite Sprite => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + GetType().Name);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.QuickFocus);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.QuickFocus);
}
