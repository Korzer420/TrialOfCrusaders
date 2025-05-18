using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class Supreme : Power
{
    private int _killedEnemies = 0;

    public int NeededEnemies => 50 - CombatController.CombatLevel - CombatController.EnduranceLevel;

    public override string Name => "Supreme";

    public override string Description => "Killing multiple enemies in a row without taking damage restores health. Can only occur once per room.";

    public override (float, float, float) BonusRates => new(6f, 0f, 4f);

    internal override void Enable()
    {
        On.HealthManager.Die += HealthManager_Die;
        On.HeroController.TakeDamage += HeroController_TakeDamage;
    }

    internal override void Disable()
    {
        On.HealthManager.Die -= HealthManager_Die;
        On.HeroController.TakeDamage -= HeroController_TakeDamage;
    }

    private void HeroController_TakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, UnityEngine.GameObject go, GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
    {
        int currentHealth = PDHelper.Health;
        orig(self, go, damageSide, damageAmount, hazardType);
        if (currentHealth != PDHelper.Health)
            _killedEnemies = 0;
    }

    private void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        orig(self, attackDirection, attackType, ignoreEvasion);
        _killedEnemies++;
        if (_killedEnemies >= NeededEnemies)
        { 
            _killedEnemies = 0;
            HeroController.instance.AddHealth(1);
        }
    }
}