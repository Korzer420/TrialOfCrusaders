using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.UnityComponents.PowerElements;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class WaywardCompass : Power
{
    public override bool CanAppear => true;

    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override DraftPool Pools => DraftPool.Endurance;

    protected override void Enable() => CombatRef.BeginCombat += CombatController_BeginCombat;

    protected override void Disable() => CombatRef.BeginCombat -= CombatController_BeginCombat;

    private void CombatController_BeginCombat()
    {
        if (StageRef.CurrentRoom.BossRoom)
            return;
        GameObject compass = new("Compass");
        compass.layer = 5;
        compass.AddComponent<SpriteRenderer>().sprite = Sprite;
        compass.transform.position = new(14, -7f);
        compass.transform.localScale = new(1.5f, 1.5f);
        compass.AddComponent<Compass>();
    }
}
