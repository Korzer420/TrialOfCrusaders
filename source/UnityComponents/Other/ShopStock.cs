using KorzUtils.Helper;
using System.Collections.Generic;
using TMPro;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Manager;
using UnityEngine;

namespace TrialOfCrusaders.UnityComponents.Other;

internal class ShopStock : MonoBehaviour
{
    private List<(string, int)> _stock = [];

    void Start()
    {

    }

    internal void GenerateShopUI(PlayMakerFSM fsm)
    {
        GameObject shopUI = new("ToC Shop");
        shopUI.layer = 5;
        shopUI.transform.position = new(0f, 0f);

        GameObject viewBlocker = new("View Blocker");
        viewBlocker.layer = 5;
        viewBlocker.transform.SetParent(shopUI.transform, true);
        viewBlocker.AddComponent<SpriteRenderer>().sprite = TreasureManager.BackgroundSprite;
        viewBlocker.GetComponent<SpriteRenderer>().sortingOrder = 1;
        viewBlocker.GetComponent<SpriteRenderer>().color = new(0f, 0f, 0f, 0.9f);
        viewBlocker.transform.localScale = new Vector3(30f, 23f, 1f);
        viewBlocker.transform.localPosition = new(-1f, -1f);
        viewBlocker.SetActive(true);

        (SpriteRenderer, TMPro.TextMeshPro) currentElement = TextManager.CreateUIObject("UpperBorder");
        currentElement.Item1.transform.SetParent(shopUI.transform);
        currentElement.Item1.transform.localPosition = new(-0.85f, 5.3f);
        currentElement.Item1.transform.localScale = new(1.5f, 1.5f);
        currentElement.Item1.sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Border.History_Upper");
        GameObject.Destroy(currentElement.Item2.gameObject);

        currentElement = TextManager.CreateUIObject("LowerBorder");
        currentElement.Item1.transform.SetParent(shopUI.transform);
        currentElement.Item1.transform.localPosition = new(-1f, -7f);
        currentElement.Item1.transform.localScale = new(1.5f, 1.5f);
        currentElement.Item1.sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Border.History_Lower");
        GameObject.Destroy(currentElement.Item2.gameObject);

        GameObject seperator = GameObject.Instantiate(InventoryController.ElementLookup[InventoryController.Selector].Item1.transform
            .parent.parent.Find("Seperator Holder/Left divider").gameObject);
        seperator.transform.SetParent(shopUI.transform);
        seperator.transform.localPosition = new(0f, -0.9f);
        seperator.transform.localScale = new(5.6477f, 0.8253f);

        

        GameObject shopList = new("Shop List");
        shopList.layer = 5;
        shopList.transform.SetParent(shopUI.transform);
        shopList.transform.localPosition = new(0.2f, -2f);
        shopList.transform.localScale = new(0.8f, 0.8f);
        for (int i = 1; i < 8; i++)
        {
            (SpriteRenderer, TextMeshPro) stock = TextManager.CreateUIObject("Stock " + i);
            GameObject.Destroy(stock.Item2.gameObject);
            stock.Item1.transform.SetParent(shopList.transform);
            stock.Item1.sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities.HowlingWraiths");
            stock.Item1.transform.localPosition = new(2f, 9.4f - i * 2f);
            stock.Item1.transform.localScale = new(1f, 1f);
            currentElement = TextManager.CreateUIObject("Stock Price "+i);
            currentElement.Item1.transform.SetParent(stock.Item1.transform);
            currentElement.Item1.transform.localPosition = new(1.5f, 0f);
        }

        GameObject selector = GameObject.Instantiate(InventoryController.ElementLookup[InventoryController.Selector].Item1.gameObject);
        selector.transform.SetParent(shopList.transform);
        selector.transform.localPosition = new(3.8f, 7.4f);
        selector.transform.localScale = new(2.5f, 1.8f);

        currentElement = TextManager.CreateUIObject("Selection");
        currentElement.Item1.sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities.FuryOfTheFallen");
        currentElement.Item2.text = "Improved Fury of the Fallen";
        currentElement.Item1.transform.SetParent(shopUI.transform);
        currentElement.Item1.transform.localPosition = new(-4.7f, 0.9f);
        currentElement.Item1.transform.localScale = new(2f,2f,2f);
        currentElement.Item2.transform.localPosition = new(-1.3f, 0.8f);
        currentElement.Item2.transform.localScale = new(1.2f, 1.2f, 1.2f);
        currentElement.Item2.fontSize = 2;

        TextMeshPro description = InventoryController.CreateTextElement();
        description.text = "This is a description of a power, so this text could be very very very very very long.";
        description.transform.SetParent(shopUI.transform);
        description.alignment = TextAlignmentOptions.Top;
        description.transform.localPosition = new(-4.5f, -7f);
    }
}