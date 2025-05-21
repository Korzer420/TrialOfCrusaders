using TrialOfCrusaders.UnityComponents;

namespace TrialOfCrusaders.Powers.Common;

internal class Supreme : Power
{
    private int _killedEnemies = 0;

    public int NeededEnemies => 50 - CombatController.CombatLevel - CombatController.EnduranceLevel;

    public override (float, float, float) BonusRates => new(6f, 0f, 4f);

    protected override void Enable()
    {
        _killedEnemies = 0;
        On.HealthManager.Die += HealthManager_Die;
        CombatController.TookDamage += CombatController_TookDamage;
    }

    protected override void Disable()
    {
        On.HealthManager.Die -= HealthManager_Die;
        CombatController.TookDamage -= CombatController_TookDamage;
    }

    private void CombatController_TookDamage() => _killedEnemies = 0;

    private void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        orig(self, attackDirection, attackType, ignoreEvasion);
        if (self.GetComponent<BaseEnemy>() != null)
        {
            _killedEnemies++;
            if (_killedEnemies >= NeededEnemies)
            {
                _killedEnemies = 0;
                HeroController.instance.AddHealth(1);
            }
        }
    }
}