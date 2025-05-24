using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Enums;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Rare;

internal class NailmastersGlory : Power
{
    public override (float, float, float) BonusRates => new(100f, 0f, 0f);

    public override Rarity Tier => Rarity.Rare;

    public override bool CanAppear => !HasPower<ShiningBound>() && CombatController.HasNailArt();

    public override Sprite Sprite => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + GetType().Name);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.NailmastersGlory);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.NailmastersGlory);
}
