using MonoMod.Cil;
using System;
using System.Collections;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.UnityComponents.Other;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ImprovedGatheringSwarm : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public override DraftPool Pools => DraftPool.Wealth | DraftPool.Charm | DraftPool.Upgrade;

    public override bool CanAppear => HasPower<GatheringSwarm>();

    protected override void Enable()
    { 
        IL.GeoControl.FixedUpdate += GeoControl_FixedUpdate;
        TreasureManager.SpawnedShiny += TreasureManager_SpawnedShiny;
    }

    protected override void Disable() 
    {
        IL.GeoControl.FixedUpdate -= GeoControl_FixedUpdate;
        TreasureManager.SpawnedShiny -= TreasureManager_SpawnedShiny;
    }

    private void TreasureManager_SpawnedShiny(TreasureType arg1, UnityEngine.GameObject arg2)
    {
        GameObject draw = new("Mover");
        draw.AddComponent<Dummy>().StartCoroutine(MoveShiny(arg2));
    }

    private void GeoControl_FixedUpdate(MonoMod.Cil.ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.Goto(0);
        cursor.GotoNext(MoveType.After, x => x.MatchLdcR4(150f));
        cursor.EmitDelegate<Func<float, float>>(x => 450f);
        cursor.GotoNext(MoveType.After, x => x.MatchLdcR4(150f));
        cursor.EmitDelegate<Func<float, float>>(x => 450f);
    }

    private static IEnumerator MoveShiny(GameObject shiny)
    {
        if (StageRef.CurrentRoom.BossRoom)
            yield break;
        Rigidbody2D rigidbody = shiny.GetComponent<Rigidbody2D>();
        while(shiny != null)
        {
            if (!HeroController.instance.acceptingInput || CombatRef.InCombat)
            {
                yield return null;
                continue;
            }

            float distance = Vector3.Distance(HeroController.instance.transform.position, shiny.transform.position);
            if (distance > 3)
            { 
                rigidbody.gravityScale = 0f;
                shiny.transform.position = Vector3.MoveTowards(shiny.transform.position, HeroController.instance.transform.position, Time.deltaTime * 4);
            }
            else
                rigidbody.gravityScale = 0.85f;
            
            yield return null;
        }
    }
}
