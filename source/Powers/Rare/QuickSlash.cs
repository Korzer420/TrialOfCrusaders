using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Rare;

internal class QuickSlash : Power
{
    public override (float, float, float) BonusRates => new(100f, 0f, 0f);

    public override Rarity Tier => Rarity.Rare;

    public override DraftPool Pools => DraftPool.Combat | DraftPool.Charm;

    public override bool CanAppear => !HasPower<ShiningBound>();

    public override Sprite Sprite => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + GetType().Name);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.QuickSlash);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.QuickSlash);
}
