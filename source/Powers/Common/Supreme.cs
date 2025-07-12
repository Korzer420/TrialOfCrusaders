using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Common;

internal class Supreme : Power
{
    private int _killedEnemies = 0;

    public int NeededEnemies => 50 - CombatRef.CombatLevel - CombatRef.EnduranceLevel;

    public override (float, float, float) BonusRates => new(6f, 0f, 4f);

    public override StatScaling Scaling => StatScaling.Combat | StatScaling.Endurance;

    public override DraftPool Pools => DraftPool.Burst | DraftPool.Endurance;

    protected override void Enable()
    {
        _killedEnemies = 0;
        CombatRef.EnemyKilled += CombatController_EnemyKilled;
        CombatRef.TookDamage += CombatController_TookDamage;
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
        CombatRef.EnemyKilled -= CombatController_EnemyKilled;
        CombatRef.TookDamage -= CombatController_TookDamage;
    }

    private void CombatController_TookDamage() => _killedEnemies = 0;
}