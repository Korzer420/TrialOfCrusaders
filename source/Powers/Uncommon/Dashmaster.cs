using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class Dashmaster : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 40f);

    public override Rarity Tier => Rarity.Uncommon;

    public override bool CanAppear => PDHelper.HasDash && !HasPower<ShiningBound>();

    public override Sprite Sprite => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + GetType().Name);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.Dashmaster);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.Dashmaster);
}
