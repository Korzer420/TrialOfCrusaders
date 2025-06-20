using KorzUtils.Helper;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrialOfCrusaders.UnityComponents.Other;

internal class SeedTablet : MonoBehaviour
{
    private float _cooldown = 0.25f;
    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer _tabletSprite;
    private static readonly List<string> _seedSprites =
    [
        "CrystalDash",
        "CycloneSlash",
        "DesolateDive",
        "GreatSlash",
        "HowlingWraiths",
        "IsmasTear",
        "MantisClaw",
        "MonarchWings",
        "MothwingCloak",
        "VengefulSpirit"
    ];

    public int Index { get; set; }

    public int Number { get; set; }

    public int InitialNumber { get; set; }

    void Start()
    {
        _tabletSprite = GetComponent<SpriteRenderer>();
        _spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + _seedSprites[Number]);
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
        _spriteRenderer.sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + _seedSprites[Number]);
        if (Number != InitialNumber)
            _tabletSprite.color = Color.red;
        else
            _tabletSprite.color = Color.white;
    }
}
