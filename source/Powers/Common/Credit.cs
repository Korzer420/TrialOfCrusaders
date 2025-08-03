using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Common;

public class Credit : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override DraftPool Pools => DraftPool.Instant | DraftPool.Wealth | DraftPool.Risk;

    public int LeftCredit { get; set; }

    protected override void Enable()
    {
        HeroController.instance.AddGeo(1000);
        CoroutineHelper.WaitFrames(() => LeftCredit = 1000, true, 300);
        On.GeoCounter.Update += GeoCounter_Update;
    }

    protected override void Disable()
    {
        LeftCredit = 0;
        On.GeoCounter.Update -= GeoCounter_Update;
    }

    private void GeoCounter_Update(On.GeoCounter.orig_Update orig, GeoCounter self)
    {
        orig(self);
        if (LeftCredit != 0)
            self.geoTextMesh.text = $"{PDHelper.Geo} (Left credit: {LeftCredit})";
    }
}