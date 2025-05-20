using KorzUtils.Helper;
using System;
using System.Collections;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.UnityComponents;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ImprovedSporeshroom : Power
{
    private GameObject _cloud;

    private GameObject _holder;

    public override (float, float, float) BonusRates => new(5f, 0f, 35f);

    public override Rarity Tier => Rarity.Uncommon;

    public override bool CanAppear => HasPower<Sporeshroom>();

    public GameObject Cloud => _cloud ??= GameObject.Find("_GameManager")?.transform.Find("GlobalPool/Knight Spore Cloud(Clone)")?.gameObject;

    protected override void Enable()
    {
        CharmHelper.EnsureEquipCharm(KorzUtils.Enums.CharmRef.Sporeshroom);
        _holder = new("Sporecloud holder");
        GameObject.DontDestroyOnLoad(_holder);
        _holder.AddComponent<Dummy>().StartCoroutine(EmitCloud());
    }

    protected override void Disable()
    {
        _holder.GetComponent<Dummy>().StopAllCoroutines();
        GameObject.Destroy(_holder);
    }

    /// <summary>
    /// Emits the spore cloud.
    /// </summary>
    private IEnumerator EmitCloud()
    {
        while (true)
        {
            if (Cloud == null)
                yield break;
            yield return new WaitForSeconds(UnityEngine.Random.Range(Math.Max(8, 75 - (CombatController.EnduranceLevel * 3 + CombatController.CombatLevel)), 91));
            GameObject newCloud = GameObject.Instantiate(Cloud, HeroController.instance.transform.position,
            Quaternion.identity);
            newCloud.SetActive(true);
            newCloud.LocateMyFSM("Control").GetState("Init").AdjustTransition("NORMAL", "Deep");
            yield return new WaitForSeconds(4.1f);
            GameObject.Destroy(newCloud);
        }
    }
}
