using KorzUtils.Enums;
using KorzUtils.Helper;
using Modding;
using TrialOfCrusaders.UnityComponents;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class Caching : Power
{
    private GameObject _activeSoulCache;

    public static GameObject SoulCache { get; set; }

    public override string Name => "Caching";

    public override string Description => "Excessive soul may manifest in a soul sphere. Only one sphere can be active at a time.";

    public override (float, float, float) BonusRates => new(0f, 10f, 0f);

    internal override void Enable()
    {
        ModHooks.SoulGainHook += ModHooks_SoulGainHook;
    }

    private int ModHooks_SoulGainHook(int arg)
    {
        int excessiveAmount = (PDHelper.MaxMP + PDHelper.MPReserveMax - PDHelper.MPCharge - PDHelper.MPReserve - arg) * -1;
        if (excessiveAmount > 0 && Random.Range(0, 50) <= CombatController.SpiritLevel)
        {
            if (_activeSoulCache != null)
                _activeSoulCache.GetComponent<SoulCache>().SoulAmount += excessiveAmount;
            else
            {
                _activeSoulCache = GameObject.Instantiate(SoulCache);
                _activeSoulCache.transform.position = HeroController.instance.transform.position;
                BoxCollider2D collider = _activeSoulCache.AddComponent<BoxCollider2D>();
                collider.size = new(1.4f, 1.4f);
                collider.isTrigger = true;
                _activeSoulCache.layer = 18;
                _activeSoulCache.AddComponent<SoulCache>().SoulAmount = excessiveAmount;
                _activeSoulCache.SetActive(true);
            }
        }
        return arg;
    }

    internal override void Disable() => CharmHelper.UnequipCharm(CharmRef.BaldurShell);
}