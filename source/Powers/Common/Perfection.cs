using KorzUtils.Helper;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Reflection;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class Perfection : Power
{
    private int _clearedRoom = 0;
    private bool _hit;
    private readonly FieldInfo _smallGeo = typeof(HealthManager).GetField("smallGeoDrops", BindingFlags.Instance | BindingFlags.NonPublic);
    private readonly FieldInfo _mediumGeo = typeof(HealthManager).GetField("mediumGeoDrops", BindingFlags.Instance | BindingFlags.NonPublic);
    private readonly FieldInfo _largeGeo = typeof(HealthManager).GetField("largeGeoDrops", BindingFlags.Instance | BindingFlags.NonPublic);

    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    protected override void Enable()
    {
        IL.HealthManager.Die += HealthManager_Die;
        StageController.RoomCleared += StageController_RoomCleared;
        CombatController.TookDamage += CombatController_TookDamage;
        _hit = false;
    }

    protected override void Disable()
    { 
        IL.HealthManager.Die -= HealthManager_Die;
        StageController.RoomCleared -= StageController_RoomCleared;
        CombatController.TookDamage -= CombatController_TookDamage;
    }

    private void HealthManager_Die(ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.Goto(0);
        cursor.GotoNext(MoveType.After, x => x.MatchStloc(8));

        for (int i = 0; i < 3; i++)
            ModifyGeo(cursor, i);
        cursor.GotoNext(MoveType.After, x => x.MatchLdcI4(0));
        // Activate geo flashing like from fragile greed.
        cursor.EmitDelegate<Func<bool, bool>>(x => true);
    }
    private void StageController_RoomCleared()
    {
        if (!_hit)
            _clearedRoom = Math.Min(60, _clearedRoom++);
        _hit = false;
    }

    private void CombatController_TookDamage() => _hit = true;

    private void ModifyGeo(ILCursor cursor, int level)
    {
        // We want to normalize the geo amount/increase without having to cast reflection each time.
        // Since we need all three geo drop amount to determine the percentage increase we need to get creative.
        // First we push all three drops onto the stack
        cursor.Emit(OpCodes.Ldarg_0); // Load reference to the current object (HealthManager)
        cursor.Emit(OpCodes.Ldfld, _smallGeo); // Load small geo onto the stack
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldfld, _mediumGeo); // Load medium geo onto the stack
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldfld, _largeGeo); // Load large geo onto the stack
        // Now take all from the stack calculate the relevant geo amount and push the result.
        switch (level)
        {
            case 0:
                cursor.EmitDelegate(CalculateSmallGeo);
                break;
            case 1:
                cursor.EmitDelegate(CalculateMediumGeo);
                break;
            default:
                cursor.EmitDelegate(CalculateLargeGeo);
                break;
        }
        // Put the calculated value in the local variable used for the appropiate geo amount.
        cursor.Emit(OpCodes.Stloc, 6 + level);
    }

    private int CalculateSmallGeo(int smallGeo, int mediumGeo, int largeGeo)
    {
        int totalAmount = smallGeo + mediumGeo * 5 + largeGeo * 25;
        int bonusAmount = Mathf.CeilToInt(totalAmount * (1 + _clearedRoom * 0.05f)) - totalAmount;
        if (bonusAmount <= 0f)
            return smallGeo;
        // Only small geo under 10.
        if (bonusAmount <= 10)
            return smallGeo + bonusAmount;
        // If more than 10 we always drop 5 small and up to 9.
        bonusAmount %= 5;
        return smallGeo + bonusAmount + 5;
    }

    private int CalculateMediumGeo(int smallGeo, int mediumGeo, int largeGeo)
    {
        int totalAmount = smallGeo + mediumGeo * 5 + largeGeo * 25;
        int bonusAmount = Mathf.CeilToInt(totalAmount * (1 + _clearedRoom * 0.05f)) - totalAmount;
        // Only amount pass 10 will be handled with medium geo.
        if (bonusAmount <= 10f)
            return mediumGeo;

        // 5-9 are always extra in small geo. We exclude them here.
        int smallGeoAmount = (bonusAmount % 5) + 5;
        // No large geo under 31 bonus.
        if (bonusAmount <= 30f)
            return mediumGeo + bonusAmount - smallGeoAmount;
        bonusAmount -= smallGeoAmount;
        mediumGeo += (bonusAmount % 25) / 5;
        return mediumGeo;
    }

    private int CalculateLargeGeo(int smallGeo, int mediumGeo, int largeGeo)
    {
        int totalAmount = smallGeo + mediumGeo * 5 + largeGeo * 25;
        int bonusAmount = Mathf.CeilToInt(totalAmount * (1 + _clearedRoom * 0.05f)) - totalAmount;
        // Only amount pass 30 will be handled with large geo.
        if (bonusAmount <= 30f)
            return largeGeo;

        // 5-9 are always extra in small geo. We exclude them here.
        bonusAmount -= (bonusAmount % 5) + 5;
        largeGeo += bonusAmount / 25;
        return largeGeo;
    }
}