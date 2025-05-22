using TrialOfCrusaders.Controller;
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
        CombatController.EnemyKilled += CombatController_EnemyKilled;
        CombatController.TookDamage += CombatController_TookDamage;
    }

    private void CombatController_EnemyKilled(HealthManager enemy)
    {
        _killedEnemies++;
        if (_killedEnemies >= NeededEnemies)
        {
            _killedEnemies = 0;
            HeroController.instance.AddHealth(1);
        }
    }

    protected override void Disable()
    {
        CombatController.EnemyKilled -= CombatController_EnemyKilled;
        CombatController.TookDamage -= CombatController_TookDamage;
    }

    private void CombatController_TookDamage() => _killedEnemies = 0;
}