using UnityEngine;

namespace TrialOfCrusaders.UnityComponents.Other;

internal class ColorShifter : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private float _timePassed = 0f;
    private int _colorRotationIndex = 0;

    public bool Rainbox { get; set; }

    public tk2dSprite GlowSprite { get; set; }

    void Start() => _spriteRenderer = GetComponent<SpriteRenderer>();

    void Update()
    {
        _timePassed += Time.deltaTime;
        if (_timePassed > 0.25f)
        {
            if (Rainbox)
            {
                _spriteRenderer.color = new(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                if (GlowSprite != null)
                    GlowSprite.color = _spriteRenderer.color;
            }
            else
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
                _spriteRenderer.color = color;
                if (GlowSprite != null)
                    GlowSprite.color = color;
            }

            _timePassed = 0f;
        }
    }
}
