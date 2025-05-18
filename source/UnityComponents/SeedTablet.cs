using KorzUtils.Helper;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrialOfCrusaders.UnityComponents;

internal class SeedTablet : MonoBehaviour
{
    private float _cooldown = 0.25f;
    private SpriteRenderer spriteRenderer;
    private static List<string> _seedSprites =
    [
        "Crystal_Dash",
        "Cyclone_Slash",
        "Desolate_Dive",
        "Great_Slash",
        "Howling_Wraiths",
        "Ismas_Tear",
        "Mantis_Claw",
        "Monarch_Wings",
        "Mothwing_Cloak",
        "Vengeful_Spirit"
    ];

    public int Index { get; set; }

    public int Number { get; set; }

    void Start()
    {
        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.AbilitySprites." + _seedSprites[Number]);
    }

    void Update() => _cooldown = Math.Max(0, _cooldown - Time.deltaTime);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag != "Nail Attack" || 0 < _cooldown)
            return;
        _cooldown = 0.25f;
        Number++;
        if (Number == 10)
            Number = 0;
        spriteRenderer.sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.AbilitySprites." + _seedSprites[Number]);
    }
}
