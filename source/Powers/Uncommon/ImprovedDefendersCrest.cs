﻿using KorzUtils.Helper;
using Modding.Utils;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.UnityComponents.Debuffs;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ImprovedDefendersCrest : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 40f);

    public override Rarity Tier => Rarity.Uncommon;

    public override bool CanAppear => HasPower<DefendersCrest>();

    public override DraftPool Pools => DraftPool.Debuff | DraftPool.Upgrade | DraftPool.Charm | DraftPool.Endurance;

    protected override void Enable()
    {
        CharmHelper.EnsureEquipCharm(KorzUtils.Enums.CharmRef.DefendersCrest);
        On.ExtraDamageable.RecieveExtraDamage += ExtraDamageable_RecieveExtraDamage;
    }

    protected override void Disable() => On.ExtraDamageable.RecieveExtraDamage -= ExtraDamageable_RecieveExtraDamage;

    private void ExtraDamageable_RecieveExtraDamage(On.ExtraDamageable.orig_RecieveExtraDamage orig, ExtraDamageable self, ExtraDamageTypes extraDamageType)
    {
        if (extraDamageType == ExtraDamageTypes.Dung || extraDamageType == ExtraDamageTypes.Dung2)
        {
            bool afflictWeakness = UnityEngine.Random.Range(1, 101) <= 5;
            if (afflictWeakness)
            {
                if (self.GetComponent<HealthManager>() != null)
                    self.gameObject.GetOrAddComponent<WeakenedEffect>().Timer += 3;
                else if (self.transform.parent != null && self.transform.parent.GetComponent<HealthManager>() != null)
                    self.transform.parent.gameObject.GetOrAddComponent<WeakenedEffect>().Timer += 3;
            }
        }
        orig(self, extraDamageType);
    }
}
