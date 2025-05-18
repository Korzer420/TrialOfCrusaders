using KorzUtils.Enums;
using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class Hiveblood : Power
{
    public override string Name => "Hiveblood";

    public override string Description => "Taking damage regenerates the last mask lost.";

    public override (float, float, float) BonusRates => new(0f, 0f, 40f);

    public override Rarity Tier => Rarity.Uncommon;

    internal override void Enable()
    {
        CharmHelper.EnsureEquipCharm(CharmRef.Hiveblood);
        // To not deal with the UI, we just toggle it.
        GameCameras.instance.hudCanvas.gameObject.SetActive(false);
        GameCameras.instance.hudCanvas.gameObject.SetActive(true);
    }

    internal override void Disable() => CharmHelper.UnequipCharm(CharmRef.Hiveblood);
}
