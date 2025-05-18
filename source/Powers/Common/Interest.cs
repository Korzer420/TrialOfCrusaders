using Modding.Utils;
using TrialOfCrusaders.UnityComponents;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class Interest : Power
{
    public override string Name => "Interest";

    public override string Description => "Geo value is slightly increased.";

    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    internal override void Enable()
    {
        On.HeroController.AddGeo += HeroController_AddGeo;
    }

    internal override void Disable()
    {
        On.HeroController.AddGeo -= HeroController_AddGeo;
    }

    private void HeroController_AddGeo(On.HeroController.orig_AddGeo orig, HeroController self, int amount)
    {
        amount = Mathf.CeilToInt(amount * 1.2f);
        orig(self, amount);
    }
}
