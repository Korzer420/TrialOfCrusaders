using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Rare;

internal class VoidHeart : Power
{
    public override string Name => "Void Heart";

    public override string Description => "???";

    public override (float, float, float) BonusRates => new(33f, 33f, 34f);

    public override Sprite Sprite => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + GetType().Name);

    public override Rarity Tier => Rarity.Rare;

    public override bool CanAppear => ScoreController.Score.KillStreakBonus > 30;
}
