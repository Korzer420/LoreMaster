using LoreMaster.Enums;
using LoreMaster.LorePowers;
using LoreMaster.Manager;
using LoreMaster.Settings;
using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using RandomizerMod.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LoreMaster.Randomizer;

/// <summary>
/// Menu page for the rando settings.
/// </summary>
public class RandomizerMenu
{
    #region Members

    internal MenuPage _mainPage;

    internal VerticalItemPanel _mainPoolsPanel;

    internal MenuElementFactory<RandomizerSettings> _menuElementFactory;

    internal SmallButton _pageButton;

    private MenuPage _powerPage;

    private GridItemPanel _firstPowerSet;

    private GridItemPanel _secondPowerSet;

    private SmallButton[] _moveButtons;

    private SmallButton _powerButton;

    internal static RandomizerMenu _instance;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the instance of the menu.
    /// </summary>
    public static RandomizerMenu Instance => _instance ??= new();

    /// <summary>
    /// Gets the maximal amount of lore that can be required for black egg temple.
    /// </summary>
    public int MaxLore
    {
        get
        {
            int maxLore = 31;
            if (RandomizerManager.Settings.RandomizeNpc)
                maxLore += 18;
            if (RandomizerManager.Settings.RandomizePointsOfInterest)
                maxLore += 7;
            return maxLore;
        }
    }

    #endregion

    #region Event handler

    /// <summary>
    /// Event handler when the main menu is closed.
    /// </summary>
    public static void OnExitMenu()
    {
        On.FixVerticalAlign.AlignText -= FixVerticalAlign_AlignText;
        _instance = null;
    }

    private bool HandleButton(MenuPage previousPage, out SmallButton button)
    {
        try
        {
            _pageButton = new(previousPage, "Lore Master");
            _pageButton.AddHideAndShowEvent(previousPage, _mainPage);
            _mainPage.BeforeGoBack += () => _pageButton.Text.color = !RandomizerManager.Settings.Enabled ? Colors.DEFAULT_COLOR : Colors.TRUE_COLOR;
            _pageButton.Text.color = !RandomizerManager.Settings.Enabled ? Colors.DEFAULT_COLOR : Colors.TRUE_COLOR;
            button = _pageButton;
            return true;
        }
        catch (Exception error)
        {
            LoreMaster.Instance.LogError("Error in Handle button: " + error.Message);
        }
        button = null;
        return true;
    }

    private void ConstructMenu(MenuPage previousPage)
    {
        try
        {
            On.FixVerticalAlign.AlignText += FixVerticalAlign_AlignText;
            _mainPage = new MenuPage("Lore Master", previousPage);
            _menuElementFactory = new MenuElementFactory<RandomizerSettings>(_mainPage, RandomizerManager.Settings);

            _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.Enabled)].MoveTo(new(0f, 500f));
            _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.DefineRefs)].MoveTo(new(0f, 400f));
            _mainPoolsPanel = new(_mainPage, new(-400, 350), 50f, true, _menuElementFactory.Elements.Skip(2).Take(5).ToArray());
            _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.RandomizeElderbugRewards)].SelfChanged += CheckIfElderbugPossible;
            new VerticalItemPanel(_mainPage, new(400, 350), 50f, true, _menuElementFactory.Elements.Skip(7).Take(4).ToArray());
            if (!RandomizerManager.Settings.RandomizeTravellers)
                _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.TravellerOrder)].Hide();
            if (!RandomizerManager.Settings.RandomizeTreasures)
                _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.ForceCompassForTreasure)].Hide();

            for (int i = 1; i < 6; i++)
                _menuElementFactory.Elements[i].SelfChanged += AdjustLorePool;
            _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.RandomizeTravellers)].SelfChanged += AdjustLorePool;

            _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.RandomizeTravellers)].SelfChanged += TravellerOrderVisiblity;
            _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.RandomizeTreasures)].SelfChanged += CompassVisiblity;
            new VerticalItemPanel(_mainPage, new(0, -0f), 50f, true, _menuElementFactory.Elements.Skip(11).Take(2).ToArray());
            _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.BlackEggTempleCondition)].MoveTo(new(-350f, -200f));
            _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.BlackEggTempleCondition)].SelfChanged += ChangeEndCondition;
            _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.NeededLore)].MoveTo(new(-350f, -275f));
            _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.NeededLore)].SelfChanged += CheckLoreCap;
            if (RandomizerManager.Settings.BlackEggTempleCondition == BlackEggTempleCondition.Dreamers)
                _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.NeededLore)].Hide();
            _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.PowerBehaviour)].MoveTo(new(350f, -200f));
            _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.PowerBehaviour)].SelfChanged += ShowPowerButton;
            GeneratePowerPage();
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Error in construct menu: " + exception.Message);
            LoreMaster.Instance.LogError("Error in construct menu: " + exception.StackTrace);
        }
    }

    private void AdjustLorePool(IValueElement obj)
    {
        if (RandomizerManager.Settings.RandomizeElderbugRewards && AvailableLore() < 55)
            _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.RandomizeElderbugRewards)].SetValue(false);
        if (RandomizerManager.Settings.BlackEggTempleCondition != BlackEggTempleCondition.Dreamers && AvailableLore() < RandomizerManager.Settings.NeededLore)
            _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.NeededLore)].SetValue(AvailableLore());
    }

    private void CheckIfElderbugPossible(IValueElement obj)
    {
        if ((bool)obj.Value && AvailableLore() < 55)
                obj.SetValue(false);
    }

    private void CheckLoreCap(IValueElement obj)
    {
        if ((int)obj.Value < 0)
            obj.SetValue(0);
        else if ((int)obj.Value > AvailableLore())
            obj.SetValue(AvailableLore());
    }

    private void ShowPowerButton(IValueElement obj)
    {
        if ((LoreSetOption)obj.Value != LoreSetOption.Custom)
            _powerButton.Hide();
        else
            _powerButton.Show();
    }

    private void CompassVisiblity(IValueElement obj)
    {
        if ((bool)obj.Value)
            _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.ForceCompassForTreasure)].Show();
        else
            _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.ForceCompassForTreasure)].Hide();
    }

    private void TravellerOrderVisiblity(IValueElement obj)
    {
        if ((bool)obj.Value)
            _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.TravellerOrder)].Show();
        else
        {
            _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.TravellerOrder)].Hide();
            _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.TravellerOrder)].SetValue(TravelOrder.Vanilla);
        }
    }

    private void ChangeEndCondition(IValueElement obj)
    {
        if ((BlackEggTempleCondition)obj.Value != BlackEggTempleCondition.Dreamers)
            _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.NeededLore)].Show();
        else
            _menuElementFactory.ElementLookup[nameof(RandomizerManager.Settings.NeededLore)].Hide();
    }

    private void ChangePowerTag(IValueElement element) => PowerManager.GlobalPowerStates[(element as MenuItem<PowerTag>).Name] = (element as MenuItem<PowerTag>).Value;

    private void LeftButton_OnClick()
    {
        _firstPowerSet.Show();
        _secondPowerSet.Hide();
        _moveButtons[0].Show();
        _moveButtons[1].Hide();
    }

    private void RightButton_OnClick()
    {
        _firstPowerSet.Hide();
        _secondPowerSet.Show();
        _moveButtons[1].Show();
        _moveButtons[0].Hide();
    }

    private void Change_PowerControl(PowerTag chosenTag)
    {
        foreach (MenuItem<PowerTag> item in _firstPowerSet.Items.Concat(_secondPowerSet.Items))
            item.SetValue(chosenTag);
    }

    private static void FixVerticalAlign_AlignText(On.FixVerticalAlign.orig_AlignText orig, FixVerticalAlign self)
    {
        orig(self);
        try
        {
            if (PowerManager.GlobalPowerStates == null || !PowerManager.GlobalPowerStates.Any())
            {
                PowerManager.GlobalPowerStates = new();
                foreach (Power power in PowerManager.GetAllPowers())
                    PowerManager.GlobalPowerStates.Add(power.PowerName, power.DefaultTag);
            }
            if (!string.IsNullOrEmpty(self.transform.parent?.name) &&
                self.transform.parent.name.Length > 7
                && PowerManager.GlobalPowerStates.ContainsKey(self.transform.parent.name.Substring(0, self.transform.parent.name.Length - 7)))
            {
                Text text = self.GetComponent<Text>();
                text.verticalOverflow = VerticalWrapMode.Overflow;
                text.lineSpacing = 1f;
            }
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Error in align text: " + exception.StackTrace);
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Attach the menu to the randomizer.
    /// </summary>
    public static void AttachMenu()
    {
        RandomizerMenuAPI.AddMenuPage(Instance.ConstructMenu, Instance.HandleButton);
        MenuChangerMod.OnExitMainMenu += OnExitMenu;
    }

    private void GeneratePowerPage()
    {
        // Power buttons
        try
        {
            _powerPage = new("Power Control", _mainPage);
            MenuItem<PowerTag>[] powers = new MenuItem<PowerTag>[60];
            List<PowerTag> tags = (Enum.GetValues(typeof(PowerTag)) as PowerTag[]).ToList();
            int index = 0;
            foreach (string key in PowerManager.GlobalPowerStates.Keys)
            {
                MenuItem<PowerTag> item = new(_powerPage, key, tags, new MultiLineFormatter());
                // This is a wacky workaround, since I can't bind on a dictionary that easily and have no clue how I'd implement that.
                item.SelfChanged += ChangePowerTag;
                powers[index] = item;
                index++;
            }
            _firstPowerSet = new(_powerPage, new(0, 370), 5, 150, 370, false, powers.Take(30).ToArray());
            _secondPowerSet = new(_powerPage, new(0, 370), 5, 150, 370, false, powers.Skip(30).ToArray());
            _secondPowerSet.Hide();

            // Add navigation
            _powerButton = new(_mainPage, "Power States");
            _powerButton.MoveTo(new(350f, -250f));
            _powerButton.AddHideAndShowEvent(_mainPage, _powerPage);
            if (RandomizerManager.Settings.PowerBehaviour != LoreSetOption.Custom)
                _powerButton.Hide();
             
            MenuItem<PowerTag> powerControl = new(_powerPage, "All Powers", tags);
            powerControl.ValueChanged += Change_PowerControl;
            _moveButtons = new SmallButton[2];
            SmallButton rightButton = new(_powerPage, ">>");
            rightButton.OnClick += RightButton_OnClick;
            _moveButtons[0] = rightButton;

            SmallButton leftButton = new(_powerPage, "<<");
            leftButton.OnClick += LeftButton_OnClick;
            _moveButtons[1] = leftButton;
            powerControl.MoveTo(new(0, 440f));
            leftButton.MoveTo(new(-700, -430));
            rightButton.MoveTo(new(700, -430));
            leftButton.Hide();

            _powerPage.Hide();

            foreach (MenuItem<PowerTag> item in _firstPowerSet.Items.Concat(_secondPowerSet.Items))
                item.SetValue(PowerManager.GlobalPowerStates[item.Name]);
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError(exception.StackTrace);
        }
    }

    /// <summary>
    /// Get how much lore is collectable.
    /// </summary>
    private int AvailableLore()
    {
        int lore = 31;
        if (RandomizerManager.Settings.RandomizeNpc || RandomizerManager.Settings.DefineRefs)
            lore += RandomizerRequestModifier.NpcItems.Length - 2;
        if (RandomizerManager.Settings.RandomizeDreamWarriorStatues || RandomizerManager.Settings.DefineRefs)
            lore += RandomizerRequestModifier.DreamWarriorItems.Length;
        if (RandomizerManager.Settings.RandomizeTravellers || RandomizerManager.Settings.DefineRefs)
            lore += RandomizerRequestModifier.TravellerItems.Length - 2;
        if (RandomizerManager.Settings.RandomizeDreamDialogue || RandomizerManager.Settings.DefineRefs)
            lore += RandomizerRequestModifier.DreamItems.Length;
        if (RandomizerManager.Settings.RandomizePointsOfInterest || RandomizerManager.Settings.DefineRefs)
            lore += RandomizerRequestModifier.PointOfInterestItems.Length;
        return lore;
    }

    /// <summary>
    /// Paste the settings from rando settings manager.
    /// </summary>
    /// <param name="settings"></param>
    public void PasteSettings(FullRandoSettings settings)
    {
        if (settings == null || settings.BaseSettings == null || settings.Tags == null)
        {
            _menuElementFactory.ElementLookup[nameof(settings.BaseSettings.Enabled)].SetValue(false);
            return;
        }
        _menuElementFactory.SetMenuValues(settings.BaseSettings);
        int index = 0;
        foreach (MenuItem<PowerTag> tag in _firstPowerSet.Items.Concat(_secondPowerSet.Items).Cast<MenuItem<PowerTag>>())
        {
            if (index >= settings.Tags.Count)
                break;
            tag.SetValue(settings.Tags[index]);
            index++;
        }
    }

    #endregion
}
