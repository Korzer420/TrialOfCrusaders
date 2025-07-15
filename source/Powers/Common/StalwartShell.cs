using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class StalwartShell : Power
{
    public override bool CanAppear => !PowerRef.HasPower<ShiningBound>(out _);

    public override (float, float, float) BonusRates => new(0,0, 10f);

    public override DraftPool Pools => DraftPool.Endurance | DraftPool.Charm;

    public override Sprite Sprite => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + GetType().Name);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.StalwartShell);

    protected override void Disable() => CharmHelper.LockCharm(CharmRef.StalwartShell);
}
