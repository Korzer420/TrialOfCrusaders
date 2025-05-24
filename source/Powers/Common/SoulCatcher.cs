using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Powers.Rare;
using TrialOfCrusaders.Powers.Uncommon;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class SoulCatcher : Power
{
    public override bool CanAppear => !CombatController.HasPower<ShiningBound>(out _);

    public override (float, float, float) BonusRates => new(0f, 10f, 0f);

    public override Sprite Sprite => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + GetType().Name);

    protected override void Enable()
    {
        if (!CombatController.HasPower<SoulEater>(out _))
            CharmHelper.EnsureEquipCharm(CharmRef.SoulCatcher);
    }

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.SoulCatcher);
}
