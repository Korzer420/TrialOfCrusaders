using KorzUtils.Enums;
using KorzUtils.Helper;
using MonoMod.Cil;
using System;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class CarefreeMelody : Power
{
    private bool _isInstaKill = false;

    public override string Name => "Carefree Melody";

    public override string Description => "Occusionally blocks damage.";

    public override (float, float, float) BonusRates => new(0f, 0f, 40f);

    public override Rarity Tier => Rarity.Uncommon;

    internal override void Enable()
    {
        // Carefree Melody should not block insta death damage.
        IL.HeroController.TakeDamage += HeroController_TakeDamage_IL;
        On.HeroController.TakeDamage += HeroController_TakeDamage;
        CharmHelper.EnsureEquipCharm(CharmRef.CarefreeMelody); 
    }

    internal override void Disable() 
    {
        IL.HeroController.TakeDamage -= HeroController_TakeDamage_IL;
        On.HeroController.TakeDamage -= HeroController_TakeDamage;
        CharmHelper.UnequipCharm(CharmRef.CarefreeMelody);
    }

    private void HeroController_TakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, UnityEngine.GameObject go, GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
    {
        _isInstaKill = damageAmount == 500;
        orig(self, go, damageSide, damageAmount, hazardType);
    }

    private void HeroController_TakeDamage_IL(ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.Goto(0);
        cursor.GotoNext(MoveType.After,
            x => x.MatchLdcI4(0),
            x => x.MatchStloc(3),
            x => x.MatchLdloc(3));
        cursor.EmitDelegate<Func<bool, bool>>(x => x && !_isInstaKill);
    }
}
