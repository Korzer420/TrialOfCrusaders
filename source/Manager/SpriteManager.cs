using KorzUtils.Helper;
using UnityEngine;

namespace TrialOfCrusaders.Manager;

internal static class SpriteManager
{
    internal static Sprite GetSprite(string spriteName)
    {
        Sprite sprite = null;
        if (TrialOfCrusaders.Instance.GlobalSettings.UseCustomSprites)
            sprite = SpriteHelper.CreateFromDisk<TrialOfCrusaders>($"Sprites/{spriteName.Replace('.', '/')}.png");
        sprite ??= SpriteHelper.Create<TrialOfCrusaders>($"TrialOfCrusaders.Resources.Sprites.{spriteName}.png");
        if (TrialOfCrusaders.Instance.GlobalSettings.UseCustomSprites)
            sprite ??= SpriteHelper.Create<TrialOfCrusaders>($"Sprites/Abilities/Placeholder.png");
        sprite ??= SpriteHelper.Create<TrialOfCrusaders>($"TrialOfCrusaders.Resources.Sprites.Abilities.Placeholder.png");
        return sprite;
    }
}
