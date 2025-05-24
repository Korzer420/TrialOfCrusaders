using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class Hiveblood : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 40f);

    public override Rarity Tier => Rarity.Uncommon;

    public override bool CanAppear => !HasPower<InUtterDarkness>() && !HasPower<ShiningBound>();

    public override Sprite Sprite => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + GetType().Name);

    protected override void Enable()
    {
        CharmHelper.EnsureEquipCharm(CharmRef.Hiveblood);
        // To not deal with the UI, we just toggle it.
        GameCameras.instance.hudCanvas.gameObject.SetActive(false);
        GameCameras.instance.hudCanvas.gameObject.SetActive(true);
    }

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.Hiveblood);
}
