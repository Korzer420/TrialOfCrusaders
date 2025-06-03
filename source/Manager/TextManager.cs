using TMPro;
using UnityEngine;

namespace TrialOfCrusaders.Manager;

internal static class TextManager
{
    /// <summary>
    /// Creates an object with a <see cref="SpriteRenderer"/> on the normal element and a <see cref="TextMeshPro"/> component on it's child.
    /// </summary>
    /// <param name="objectName">The name the main object should have. The child will be named the same with a "_Text" suffix.</param>
    internal static (SpriteRenderer, TextMeshPro) CreateUIObject(string objectName)
    {
        GameObject prefab = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Inv_Items/Geo").gameObject;
        GameObject uiElement = Object.Instantiate(prefab);
        uiElement.GetComponent<SpriteRenderer>().enabled = true;
        uiElement.GetComponent<SpriteRenderer>().color = new(1f, 1f, 1f, 1f);
        uiElement.name = objectName;
        TextMeshPro text = uiElement.GetComponent<DisplayItemAmount>().textObject;
        text.GetComponent<MeshRenderer>().enabled = true;
        text.color = new(1f, 1f, 1f, 1f);
        text.gameObject.name = objectName + "_Text";
        Object.Destroy(uiElement.GetComponent<DisplayItemAmount>());
        return new(uiElement.GetComponent<SpriteRenderer>(), text);
    }
}
