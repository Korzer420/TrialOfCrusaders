using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Rare;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class GrubberflysElegy : Power
{
    public override bool CanAppear => !CombatRef.HasPower<ShiningBound>(out _);

    public override string Name => "Grubberfly's Elegy";

    public override (float, float, float) BonusRates => new(5f, 0f, 5f);

    public override DraftPool Pools => DraftPool.Combat | DraftPool.Charm;

    public override Sprite Sprite => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + GetType().Name);

    protected override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.GrubberflyElegy);

    protected override void Disable() => CharmHelper.UnequipCharm(CharmRef.GrubberflyElegy);
}
