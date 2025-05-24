using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Rare;

internal class ShamanStone : Power
{
    public override (float, float, float) BonusRates => new(0f, 100f, 0f);

    public override Rarity Tier => Rarity.Rare;

    public override bool CanAppear => !HasPower<ShiningBound>();

    public override Sprite Sprite => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + GetType().Name);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.ShamanStone);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.ShamanStone);
}
