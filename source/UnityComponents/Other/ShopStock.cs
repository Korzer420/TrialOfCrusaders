using KorzUtils.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;
using TrialOfCrusaders.Powers.Uncommon;
using TrialOfCrusaders.Resources.Text;
using UnityEngine;
using static TrialOfCrusaders.ControllerShorthands;

namespace TrialOfCrusaders.UnityComponents.Other;

internal class ShopStock : MonoBehaviour
{
    private List<(string, int, StockState)> _stock = [];
    private GameObject _shopUI;
    private Dictionary<string, GameObject> _elementLookup = [];
    private PlayMakerFSM _tukFsm;
    private float _cooldown = 0f;
    private int _itemIndex = 0;
    private const string CombatStatStock = "Stat_Combat";
    private const string SpiritStatStock = "Stat_Spirit";
    private const string EnduranceStatStock = "Stat_Endurance";
    private const string TeaStock = "Consumable_Tea";
    private const string NailStock = "Consumable_Nail";
    private const string EggStock = "Consumable_Egg";
    private const string LifebloodStock = "Consumable_Lifeblood";
    private const string SealStock = "Consumable_Seal";

    private const string SelectionTitle = "Selection_Title";
    private const string SelectionSprite = "Selection_Sprite";
    private const string SelectionDescription = "Selection_Description";
    private const string SelectionSold = "Selection_Sold";

    private readonly string[] _consumableGroup =
    [
        TeaStock,
        NailStock,
        EggStock,
        LifebloodStock,
        SealStock
    ];
    private readonly string[] _statGroup =
    [
        CombatStatStock,
        SpiritStatStock,
        EnduranceStatStock
    ];

    private string SelectedItemName => _stock[_itemIndex].Item1;

    private int SelectedItemCost => _stock[_itemIndex].Item2;

    void Start()
    {
        _tukFsm = gameObject.LocateMyFSM("Conversation Control");
        GenerateStock();
    }

    void Update()
    {
        if (_shopUI == null)
            return;
        if (_cooldown > 0f)
        {
            _cooldown -= Time.deltaTime;
            if (_cooldown < 0f)
                _cooldown = 0f;
        }
        else
        {
            if (InputHandler.Instance.inputActions.down.IsPressed && _itemIndex >= 0)
            {
                _cooldown = 0.25f;
                _itemIndex++;
                if (_itemIndex == 3 + SecretRef.ShopLevel)
                    _itemIndex = 0;
                UpdateSelection(_itemIndex);
            }
            else if (InputHandler.Instance.inputActions.up.IsPressed && _itemIndex >= 0)
            {
                _cooldown = 0.25f;
                _itemIndex--;
                if (_itemIndex == -1)
                    _itemIndex = 2 + SecretRef.ShopLevel;

                UpdateSelection(_itemIndex);
            }
            else if (InputHandler.Instance.inputActions.menuSubmit.WasPressed || InputHandler.Instance.inputActions.jump.WasPressed)
            {
                if (_itemIndex >= 0)
                    Purchase();
                else if (_itemIndex == -1)
                {
                    GenerateStock(true);
                    UpdatePrices();
                    UpdateSelection(_itemIndex);
                }
            }
            else if (InputHandler.Instance.inputActions.menuCancel.WasPressed || InputHandler.Instance.inputActions.focus.WasPressed)
            {
                GameObject.Destroy(_shopUI);
                _tukFsm.SendEvent("CONVO_FINISH");
                _itemIndex = 0;
            }
            else if (InputHandler.Instance.inputActions.left.IsPressed || InputHandler.Instance.inputActions.right.IsPressed)
            {
                _cooldown = 0.25f;
                if (_itemIndex >= 0)
                {
                    if (SecretRef.ShopLevel == 4)
                    {
                        _itemIndex = -1;
                        _elementLookup["Selector"].transform.localPosition = new(-6f, -3.8f);
                        _elementLookup["Selector"].transform.localScale = new(2.5f, 0.9f);
                    }
                }
                else
                {
                    _itemIndex = 0;
                    UpdateSelection(_itemIndex);
                }
            }
        }
    }

    internal void GenerateShopUI()
    {
        _itemIndex = 0;
        _elementLookup.Clear();
        GameObject shopUI = new("ToC Shop")
        {
            layer = 5
        };
        shopUI.transform.position = new(7f, 0f);

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
        for (int i = 1; i <= _stock.Count; i++)
        {
            (SpriteRenderer, TextMeshPro) stock = TextManager.CreateUIObject("Stock " + i);
            GameObject.Destroy(stock.Item2.gameObject);
            stock.Item1.transform.SetParent(shopList.transform);
            stock.Item1.sprite = GenerateSprite(i);
            stock.Item1.transform.localPosition = new(1.5f, 9.4f - i * 2f);
            stock.Item1.transform.localScale = new(0.9f, 0.9f);
            currentElement = TextManager.CreateUIObject("Stock Price " + i);
            currentElement.Item1.transform.SetParent(stock.Item1.transform);
            currentElement.Item1.transform.localPosition = new(2.5f, 0f);
            _elementLookup.Add("Stock Price " + i, currentElement.Item2.gameObject);
        }

        GameObject selector = GameObject.Instantiate(InventoryController.ElementLookup[InventoryController.Selector].Item1.gameObject);
        selector.transform.SetParent(shopList.transform);
        selector.transform.localPosition = new(3.8f, 7.4f);
        selector.transform.localScale = new(2.5f, 1.8f);
        _elementLookup.Add("Selector", selector);

        currentElement = TextManager.CreateUIObject("Selection");
        currentElement.Item1.sprite = GenerateSprite(1);
        currentElement.Item2.text = _stock[0].Item1;
        currentElement.Item1.transform.SetParent(shopUI.transform);
        currentElement.Item1.transform.localPosition = new(-4.7f, 0.9f);
        currentElement.Item1.transform.localScale = new(1.5f, 1.5f, 1.5f);
        currentElement.Item2.transform.localPosition = new(0f, 1f);
        currentElement.Item2.transform.localScale = new(2f, 2f, 2f);
        currentElement.Item2.fontSize = 2;
        currentElement.Item2.alignment = TextAlignmentOptions.Top;
        _elementLookup.Add(SelectionSprite, currentElement.Item1.gameObject);
        _elementLookup.Add(SelectionTitle, currentElement.Item2.gameObject);

        TextMeshPro description = InventoryController.CreateTextElement();
        description.text = "This is a description of a power, so this text could be very very very very very long.";
        description.transform.SetParent(shopUI.transform);
        description.alignment = TextAlignmentOptions.Top;
        description.textContainer.size = new(8f, 10f);
        description.transform.localPosition = new(-4.5f, -7f);
        _elementLookup.Add(SelectionDescription, description.gameObject);

        description = InventoryController.CreateTextElement(true);
        description.text = "NOT AVAILABLE";
        description.color = Color.red;
        description.fontSize = 9;
        description.transform.SetParent(_elementLookup[SelectionSprite].transform);
        description.alignment = TextAlignmentOptions.Top;
        description.textContainer.size = new(8f, 10f);
        description.transform.localPosition = new(0f, -4.7f);
        description.gameObject.SetActive(false);
        _elementLookup.Add(SelectionSold, description.gameObject);

        if (SecretRef.ShopLevel == 4)
        {
            description = InventoryController.CreateTextElement();
            description.gameObject.SetActive(true);
            description.text = "RESTOCK (+20%)";
            description.fontSize = 4;
            description.fontStyle = FontStyles.Bold;
            description.transform.SetParent(shopUI.transform);
            description.alignment = TextAlignmentOptions.Top;
            description.textContainer.size = new(8f, 10f);
            description.transform.localPosition = new(-4.5f, -9.75f);
        }

        _shopUI = shopUI;
        UpdateSelection(0);
        UpdatePrices();
    }

    private Sprite GenerateSprite(int index)
    {
        index--;
        string item = _stock[index].Item1;
        try
        {
            if (_consumableGroup.Contains(item))
                return SpriteManager.GetSprite($"Consumables.{item}");
            else if (_statGroup.Contains(item))
            {
                string stat = item.Substring("Stat_".Length);
                return SpriteManager.GetSprite($"Icons.{stat}_Icon");
            }
            else
                return TreasureManager.Powers.First(x => x.GetType().Name == item).Sprite;
        }
        catch (Exception ex)
        {
            LogManager.Log("Failed to generate sprite. The item is: " + item, ex);
            throw;
        }
    }

    private void UpdateSelection(int itemIndex)
    {
        int index = Math.Max(0, itemIndex);
        string itemName = _stock[index].Item1;

        // Update sprite
        _elementLookup[SelectionSprite].GetComponent<SpriteRenderer>().sprite = _elementLookup["Stock Price " + (index + 1)]
                .transform.parent.parent.GetComponent<SpriteRenderer>().sprite;

        if (_consumableGroup.Contains(itemName) || _statGroup.Contains(itemName))
        {
            _elementLookup[SelectionTitle].GetComponent<TextMeshPro>().text = ShopText.ResourceManager.GetString(itemName + "_Title");
            _elementLookup[SelectionDescription].GetComponent<TextMeshPro>().text = itemName == EggStock
                ? string.Format(ShopText.ResourceManager.GetString(itemName + "_Desc"), ConsumableRef.EggHeal)
                : itemName == LifebloodStock
                    ? string.Format(ShopText.ResourceManager.GetString(itemName + "_Desc"), ConsumableRef.LifebloodHeal)
                    : ShopText.ResourceManager.GetString(itemName + "_Desc");
        }
        else
        {
            Power power = TreasureManager.Powers.First(x => x.GetType().Name == itemName);
            _elementLookup[SelectionTitle].GetComponent<TextMeshPro>().text = power.Name;
            _elementLookup[SelectionDescription].GetComponent<TextMeshPro>().text = power.Description;
        }

        if (itemIndex >= 0)
        {
            _elementLookup["Selector"].transform.localPosition = new(3.8f, 7.4f - index * 2f);
            _elementLookup["Selector"].transform.localScale = new(2.5f, 1.8f);
            _elementLookup[SelectionSold].SetActive(SelectedItemCost == -1);
        }
        else
            _elementLookup[SelectionSold].SetActive(_stock[0].Item2 == -1);
    }

    private IEnumerator FlickerSelection()
    {
        float time = 0f;
        int steps = 0;
        while (steps <= 5)
        {
            time += Time.deltaTime;
            if (time >= 0.1f)
            {
                steps++;
                if (steps % 2 == 0)
                    _elementLookup["Selector"].GetComponent<SpriteRenderer>().color = Color.red;
                else
                    _elementLookup["Selector"].GetComponent<SpriteRenderer>().color = Color.white;
                time = 0f;
            }
            yield return null;
        }
        _elementLookup["Selector"].GetComponent<SpriteRenderer>().color = Color.white;
    }

    internal void UpdatePrices()
    {
        for (int i = 0; i < _stock.Count; i++)
        {
            TextMeshPro currentElement = _elementLookup["Stock Price " + (i + 1)].GetComponent<TextMeshPro>();
            if (_stock[i].Item2 == -1)
                currentElement.text = $"-";
            else if (PDHelper.Geo < _stock[i].Item2)
                currentElement.text = $"<color=#fa0000>{_stock[i].Item2}</color>";
            else
            {
                currentElement.text = $"{_stock[i].Item2}";
                switch (_stock[i].Item3)
                {
                    case StockState.Cheaper:
                        currentElement.text = $"<color=#03fc52>{currentElement.text}</color>";
                        break;
                    case StockState.Expensive:
                        currentElement.text = $"<color=#f59505>{currentElement.text}</color>";
                        break;
                    default:
                        break;
                }
            }
        }

    }

    private void Purchase()
    {
        _cooldown = 0.6f;
        if (SelectedItemCost == -1)
            return;
        if (PDHelper.Geo < SelectedItemCost)
            StartCoroutine(FlickerSelection());
        else
        {
            HeroController.instance.TakeGeo(SelectedItemCost);
            _stock[_itemIndex] = new(SelectedItemName, -1, StockState.Normal);
            _elementLookup["Stock Price " + (_itemIndex + 1)].GetComponent<TextMeshPro>().text = "-";

            if (_statGroup.Contains(SelectedItemName))
                switch (SelectedItemName)
                {
                    case CombatStatStock:
                        TreasureManager.GrantCombatLevel();
                        break;
                    case SpiritStatStock:
                        TreasureManager.GrantSpiritLevel();
                        break;
                    default:
                        TreasureManager.GrantEnduranceLevel();
                        break;
                }
            else if (_consumableGroup.Contains(SelectedItemName))
            {
                if (SelectedItemName == NailStock)
                {
                    ConsumableRef.EmpoweredHits += 5;
                    PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
                }
                else if (SelectedItemName == SealStock)
                    ConsumableRef.RerollSeals++;
                else if (SelectedItemName == TeaStock)
                    ConsumableRef.TeaSpell += 10;
                else if (SelectedItemName == LifebloodStock)
                {
                    for (int i = 0; i < ConsumableRef.LifebloodHeal; i++)
                        EventRegister.SendEvent("ADD BLUE HEALTH");
                    ConsumableRef.UsedLifeblood++;
                }
                else
                {
                    HeroController.instance.AddHealth(Math.Max(5, 25 - ConsumableRef.UsedEggs * 5));
                    ConsumableRef.UsedEggs++;
                }
            }
            else
            {
                Power pickedPower = TreasureManager.Powers.First(x => x.GetType().Name == SelectedItemName);
                if (pickedPower.CanAppear)
                {
                    PowerRef.ObtainedPowers.Add(pickedPower);
                    pickedPower.EnablePower();
                }
            }
            InventoryController.UpdateList(0);
            InventoryController.UpdateStats();
            UpdateSelection(_itemIndex);
            UpdatePrices();
        }
    }

    private void GenerateStock(bool restock = false)
    {
        // Create a copy for availability purposes.
        List<Power> obtainedPowers = [.. PowerRef.ObtainedPowers];
        try
        {
            List<Power> shopPowers = [];
            int abilityCount = 0;
            SecretRef.ShopLevel = 4;
            int maxAbility = 1 + SecretRef.ShopLevel / 2;
            int stockAmount = 3 + SecretRef.ShopLevel;

            int possibleCombatStat = CombatRef.CombatLevel;
            int possibleSpiritStat = CombatRef.SpiritLevel;
            int possibleEnduranceStat = CombatRef.EnduranceLevel;

            List<string> availablePowerNames = [.. TreasureManager.Powers.Where(x => x.CanAppear).Select(x => x.Name)];
            List<string> obtainedPowerNames = [.. obtainedPowers.Select(x => x.Name)];
            availablePowerNames = [.. availablePowerNames.Except(obtainedPowerNames)];
            List<Power> availablePowers = [];
            foreach (string powerName in availablePowerNames)
            {
                Power power = TreasureManager.Powers.First(x => x.Name == powerName);
                // Wealth power are not available in shops.
                if (!power.Pools.HasFlag(DraftPool.Wealth))
                    availablePowers.Add(power);
            }

            float powerChance = 10f;
            if (PowerRef.HasPower<RelicSeeker>(out _))
                powerChance *= 2;
            for (int i = 1; i <= stockAmount; i++)
            {
                if (restock && (_stock[i - 1].Item2 == -1 || _stock[i - 1].Item2 > PDHelper.Geo))
                    continue;
                float rolled = RngManager.GetRandom(0f, 100f);
                int price = 0;
                string itemName = null;
                if (rolled < powerChance && abilityCount < maxAbility || (i == stockAmount && abilityCount == 0))
                {
                    int currentAbilityCount = abilityCount;
                    abilityCount++;
                    List<Power> powers = [];
                    Rarity selectedRarity = Rarity.Common;
                    if (rolled <= powerChance / 10f)
                        selectedRarity = Rarity.Rare;
                    else if (rolled <= powerChance / 2.5f)
                        selectedRarity = Rarity.Uncommon;
                    foreach (Power power in availablePowers)
                    {
                        if (power.Tier != selectedRarity)
                            continue;
                        // This should ensure that powers can only be added to the shop if they:
                        // Don't restrict other appearing powers in the shop.
                        // Are not reliant on another power in the shop.
                        if (shopPowers.Count > 0)
                        {
                            PowerRef.ObtainedPowers.AddRange(shopPowers);
                            bool canAppear = power.CanAppear;
                            PowerRef.ObtainedPowers.Add(power);
                            canAppear = shopPowers.All(x => x.CanAppear);
                            // Reset powers
                            PowerRef.ObtainedPowers = [.. obtainedPowers];
                            if (!canAppear)
                                continue;
                        }
                        powers.Add(power);
                    }

                    if (powers.Count != 0)
                    {
                        Power selectedPower = powers[RngManager.GetRandom(0, powers.Count - 1)];
                        price = restock
                            ? Mathf.RoundToInt(_stock[i - 1].Item2 * 1.2f)
                            : selectedPower.Tier switch
                            {
                                Rarity.Rare => RngManager.GetRandom(500, 1000),
                                Rarity.Uncommon => RngManager.GetRandom(250, 500),
                                _ => RngManager.GetRandom(100, 300)
                            };
                        itemName = selectedPower.GetType().Name;
                        shopPowers.Add(selectedPower);
                        availablePowers.RemoveAll(x => x.GetType().Name == itemName);
                    }
                }

                if (price == 0)
                {
                    if (rolled < 40f && (possibleCombatStat < 20 || possibleSpiritStat < 20 || possibleEnduranceStat < 20))
                    {
                        int chosenStat = 1;
                        if (rolled <= 20f && possibleCombatStat < 20)
                            chosenStat = 1;
                        else if (rolled <= 30f && possibleSpiritStat < 20)
                            chosenStat = 2;
                        else if (possibleEnduranceStat < 20)
                            chosenStat = 3;
                        else if (possibleSpiritStat < 20)
                            chosenStat = 2;

                        price = restock
                            ? Mathf.RoundToInt(_stock[i - 1].Item2 * 1.2f)
                            : RngManager.GetRandom(100, 250);
                        price -= (price % 10);
                        // We add 20 to prevent the mod from rolling the same stat twice (unless the stat is 0).
                        switch (chosenStat)
                        {
                            case 1:
                                itemName = CombatStatStock;
                                possibleCombatStat += 20;
                                break;
                            case 2:
                                itemName = SpiritStatStock;
                                possibleSpiritStat += 20;
                                break;
                            default:
                                itemName = EnduranceStatStock;
                                possibleEnduranceStat += 20;
                                break;
                        }
                    }
                    else
                    {
                        // Determine consumable.
                        rolled = RngManager.GetRandom(1, 5);
                        itemName = rolled switch
                        {
                            1 => NailStock,
                            2 => LifebloodStock,
                            3 => EggStock,
                            4 => TeaStock,
                            _ => SealStock
                        };
                        
                        price = restock
                            ? Mathf.RoundToInt(_stock[i - 1].Item2 * 1.2f)
                            : RngManager.GetRandom(50, 200);

                        if (!restock && rolled == 5)
                            price *= 3;
                    }
                }

                price -= (price % 10);
                
                if (!restock)
                    _stock.Add(new(itemName, price, StockState.Normal));
                else
                    _stock[i - 1] = new(itemName, price, StockState.Expensive);
            }

            if (stockAmount >= 5 && !restock)
            {
                int rolledDiscount = RngManager.GetRandom(1, stockAmount);
                _stock[rolledDiscount - 1] = new(_stock[rolledDiscount - 1].Item1, _stock[rolledDiscount - 1].Item2 / 2, StockState.Cheaper);
            }

            if (restock)
                for (int i = 1; i <= stockAmount; i++)
                    _elementLookup["Stock Price " + i].transform.parent.parent.GetComponent<SpriteRenderer>().sprite = GenerateSprite(i);
            else
            {
                if (PowerRef.HasPower<Discount>(out _))
                    for (int i = 0; i < stockAmount; i++)
                        _stock[i] = new(_stock[i].Item1, Mathf.CeilToInt(_stock[i].Item2 * 0.7f), StockState.Cheaper);
                if (PowerRef.HasPower<RelicSeeker>(out _))
                    for (int i = 0; i < stockAmount; i++)
                        _stock[i] = new(_stock[i].Item1, Mathf.CeilToInt(_stock[i].Item2 * 1.5f), StockState.Expensive);
            }
        }
        catch (Exception ex)
        {
            LogManager.Log("Error setting up the shop", ex);
        }
        finally
        {
            PowerRef.ObtainedPowers = obtainedPowers;
        }
    }
}