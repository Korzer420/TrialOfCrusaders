using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;

namespace TrialOfCrusaders.Powers.Common;

internal class SeethingLifeblood : Power
{
    private bool _takenDamage;

    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    public override StatScaling Scaling => StatScaling.Endurance;

    public override DraftPool Pools => DraftPool.Endurance;

    protected override void Enable()
    {
        StageController.RoomEnded += StageController_RoomCleared;
        CombatController.TookDamage += CombatController_TookDamage;
    }

    private void StageController_RoomCleared(bool quietRoom, bool traversed)
    {
        if (!quietRoom && !_takenDamage && RngManager.GetRandom(1, 40) <= CombatController.EnduranceLevel)
            EventRegister.SendEvent("ADD BLUE HEALTH");
        _takenDamage = false;
    }

    protected override void Disable()
    {
        StageController.RoomEnded -= StageController_RoomCleared;
        CombatController.TookDamage -= CombatController_TookDamage;
    }

    private void CombatController_TookDamage() => _takenDamage = true;
}