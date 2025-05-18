using HutongGames.PlayMaker.Actions;
using KorzUtils.Helper;
using MonoMod.Cil;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.UnityComponents;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ImprovedDefendersCrest : Power
{
    public override string Name => "Improved Defender's Crest";

    public override string Description => "Enemies hit by the cloud may become weakend.";

    public override (float, float, float) BonusRates => new(0f, 0f, 40f);

    public override Rarity Tier => Rarity.Uncommon;

    internal override void Enable()
    {
        CharmHelper.EnsureEquipCharm(KorzUtils.Enums.CharmRef.DefendersCrest);
        On.ExtraDamageable.RecieveExtraDamage += ExtraDamageable_RecieveExtraDamage;
    }

    private void ExtraDamageable_RecieveExtraDamage(On.ExtraDamageable.orig_RecieveExtraDamage orig, ExtraDamageable self, ExtraDamageTypes extraDamageType)
    {
        if (extraDamageType == ExtraDamageTypes.Dung || extraDamageType == ExtraDamageTypes.Dung2)
        {
            bool afflictWeakness = UnityEngine.Random.Range(1, 101) == 1;
            if (afflictWeakness)
            {
                if (self.GetComponent<HealthManager>() != null && self.GetComponent<WeakendEffect>() == null)
                    self.gameObject.AddComponent<WeakendEffect>();
                else if (self.transform.parent?.GetComponent<HealthManager>() != null && self.transform.parent?.GetComponent<WeakendEffect>() == null)
                    self.transform.parent.gameObject.AddComponent<WeakendEffect>();
            }
        }
        orig(self, extraDamageType);
    }

    internal override void Disable()
    { }
}
