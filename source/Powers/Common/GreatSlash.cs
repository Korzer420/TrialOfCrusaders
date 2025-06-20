using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class GreatSlash : Power
{
    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    public override DraftPool Pools => DraftPool.Combat | DraftPool.Ability;

    public override Sprite Sprite => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + GetType().Name);

    protected override void Enable()
    {
        PDHelper.HasDashSlash = true;
        PDHelper.HasNailArt = true;
    }

    protected override void Disable()
    {
        PDHelper.HasDashSlash = false;
        PDHelper.HasNailArt = false;
    }
}
