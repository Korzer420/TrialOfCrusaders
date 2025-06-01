using KorzUtils.Helper;
using System.Collections.Generic;
using TrialOfCrusaders.Controller;
using UnityEngine;

namespace TrialOfCrusaders.UnityComponents;

internal class RoyalMark : MonoBehaviour
{
    public HealthManager AttachedEnemy { get; set; }

    void Start()
    {
        if (GetComponent<SpriteRenderer>() is not SpriteRenderer renderer)
            renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 1;
        renderer.sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Other.KingsBrand");
    }

    internal void CorrectPosition(HealthManager newEnemy)
    {
        transform.SetParent(newEnemy.transform);
        if (newEnemy.GetComponent<BoxCollider2D>() is BoxCollider2D boxCollider)
            transform.localPosition = new(0f, boxCollider.size.y + 1f, -0.1f);
        else
            transform.localPosition = new(0f, 2f, -0.1f);
        transform.localRotation = Quaternion.identity;
    }
}
