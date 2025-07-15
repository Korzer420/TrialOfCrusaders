using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class BaldurShell : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    public override bool CanAppear => !PowerRef.HasPower<ShiningBound>(out _);

    public override Sprite Sprite => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + GetType().Name);

    public override DraftPool Pools => DraftPool.Endurance | DraftPool.Upgrade | DraftPool.Charm;

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.BaldurShell);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.BaldurShell);
}
