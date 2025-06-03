using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Uncommon;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Rare;

internal class AbyssShriek : Power
{
    public override (float, float, float) BonusRates => new(0f, 100f, 0f);

    public override Rarity Tier => Rarity.Rare;

    public override bool CanAppear => CombatController.HasPower<HowlingWraiths>(out _);

    public override Sprite Sprite => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + GetType().Name);

    protected override void Enable()
    {
        PDHelper.ScreamLevel = 2;
        PDHelper.HasSpell = true;
    }

    protected override void Disable()
    {
        PDHelper.ScreamLevel = 0;
        PDHelper.HasSpell = false;
    }
}
