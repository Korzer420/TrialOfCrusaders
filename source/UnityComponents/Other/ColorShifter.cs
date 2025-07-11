﻿using UnityEngine;

namespace TrialOfCrusaders.UnityComponents.Other;

internal class ColorShifter : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private tk2dSprite _tk2dSprite;
    private float _timePassed = 0f;
    private int _colorRotationIndex = 0;

    public bool Rainbow { get; set; }

    public tk2dSprite GlowSprite { get; set; }

    void Start()
    { 
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
            _tk2dSprite = GetComponent<tk2dSprite>();
    }

    void Update()
    {
        _timePassed += Time.deltaTime;
        if (Rainbow)
        {
            if (_timePassed > 1f)
                _timePassed = 0f;
            if (_spriteRenderer != null)
                _spriteRenderer.color = Color.HSVToRGB(_timePassed, 1, 1);
            if (_tk2dSprite != null)
                _tk2dSprite.color = Color.HSVToRGB(_timePassed, 1, 1);
            if (GlowSprite != null)
                GlowSprite.color = _spriteRenderer.color;
        }
        else if (_timePassed > 0.25f)
        {
            _colorRotationIndex++;
            if (_colorRotationIndex == 3)
                _colorRotationIndex = 0;
            Color color = _colorRotationIndex switch
            {
                0 => Color.red,
                1 => new(1f, 0f, 1f, 1f),
                _ => Color.green,
            };
            if (_spriteRenderer != null)
                _spriteRenderer.color = color;
            if (_tk2dSprite != null)
                _tk2dSprite.color = color;
            if (GlowSprite != null)
                GlowSprite.color = color;
            _timePassed = 0f;
        }
    }
}
