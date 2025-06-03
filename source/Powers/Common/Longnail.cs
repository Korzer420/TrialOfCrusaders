using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Powers.Rare;
using TrialOfCrusaders.Powers.Uncommon;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class Longnail : Power
{
    public override bool CanAppear => !CombatController.HasPower<ShiningBound>(out _);

    public override (float, float, float) BonusRates => new(7.5f, 0f, 2.5f);

    public override Sprite Sprite => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + GetType().Name);

    protected override void Enable()
    { 
        if (!CombatController.HasPower<MarkOfPride>(out _))
            CharmHelper.EnsureEquipCharm(CharmRef.Longnail);
    }

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.Longnail);
}
