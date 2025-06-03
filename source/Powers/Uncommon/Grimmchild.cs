using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class Grimmchild : Power
{
    public override (float, float, float) BonusRates => new(0f, 20f, 20f);

    public override Rarity Tier => Rarity.Uncommon;

    public override bool CanAppear => !HasPower<CarefreeMelody>() && !HasPower<ShiningBound>();

    public override Sprite Sprite => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + GetType().Name);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.Grimmchild3);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.Grimmchild3);
}
