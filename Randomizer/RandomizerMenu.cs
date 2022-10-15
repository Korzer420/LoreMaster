using LoreMaster.Enums;
using LoreMaster.Manager;
using LoreMaster.Settings;
using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using Modding;
using RandomizerMod.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        On.FixVerticalAlign.AlignText -= _instance.FixVerticalAlign_AlignText;
        _instance = null;
    }

    private bool HandleButton(MenuPage previousPage, out SmallButton button)
    {
        try
        {
            _pageButton = new(previousPage, "Lore Master");
            _pageButton.AddHideAndShowEvent(previousPage, _mainPage);
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
            _menuElementFactory.Elements[0].MoveTo(new(0f, 400f));
            _mainPoolsPanel = new(_mainPage, new(-400, 350), 50f, true, _menuElementFactory.Elements.Skip(1).Take(5).ToArray());
            _menuElementFactory.Elements[5].SelfChanged += CheckIfElderbugPossible;
            new VerticalItemPanel(_mainPage, new(400, 350), 50f, true, _menuElementFactory.Elements.Skip(6).Take(4).ToArray());
            if (!RandomizerManager.Settings.RandomizeTravellers)
                _menuElementFactory.Elements[7].Hide();
            if (!RandomizerManager.Settings.RandomizeTreasures)
                _menuElementFactory.Elements[9].Hide();

            for (int i = 0; i < 5; i++)
                _menuElementFactory.Elements[i].SelfChanged += AdjustLorePool;
            _menuElementFactory.Elements[6].SelfChanged += AdjustLorePool;

            _menuElementFactory.Elements[6].SelfChanged += TravellerOrderVisiblity;
            _menuElementFactory.Elements[8].SelfChanged += CompassVisiblity;
            new VerticalItemPanel(_mainPage, new(0, -0f), 50f, true, _menuElementFactory.Elements.Skip(10).Take(2).ToArray());
            _menuElementFactory.Elements[12].MoveTo(new(-350f, -200f));
            _menuElementFactory.Elements[12].SelfChanged += ChangeEndCondition;
            _menuElementFactory.Elements[13].MoveTo(new(-350f, -275f));
            _menuElementFactory.Elements[13].SelfChanged += CheckLoreCap;
            if (RandomizerManager.Settings.BlackEggTempleCondition == BlackEggTempleCondition.Dreamers)
                _menuElementFactory.Elements[13].Hide();
            _menuElementFactory.Elements[14].MoveTo(new(350f, -200f));
            _menuElementFactory.Elements[14].SelfChanged += ShowPowerButton;

            if (ModHooks.GetMod("ConnectionSettingsCode", true) is Mod mod)
                ConnectionSettingManager.CreateSettingsCode(_mainPage, _menuElementFactory.Elements);
            GeneratePowerPage();
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Error in construct menu: " + exception.Message);
        }
    }

    private void AdjustLorePool(IValueElement obj)
    {
        if (RandomizerManager.Settings.RandomizeElderbugRewards && AvailableLore() < 55)
            _menuElementFactory.Elements[5].SetValue(false);
        if (RandomizerManager.Settings.BlackEggTempleCondition != BlackEggTempleCondition.Dreamers && AvailableLore() < RandomizerManager.Settings.NeededLore)
            _menuElementFactory.Elements[13].SetValue(AvailableLore());
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
            _menuElementFactory.Elements[9].Show();
        else
            _menuElementFactory.Elements[9].Hide();
    }

    private void TravellerOrderVisiblity(IValueElement obj)
    {
        if ((bool)obj.Value)
            _menuElementFactory.Elements[7].Show();
        else
            _menuElementFactory.Elements[7].Hide();
    }

    private void ChangeEndCondition(IValueElement obj)
    {
        if ((BlackEggTempleCondition)obj.Value != BlackEggTempleCondition.Dreamers)
            _menuElementFactory.Elements[13].Show();
        else
            _menuElementFactory.Elements[13].Hide();
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

    private void FixVerticalAlign_AlignText(On.FixVerticalAlign.orig_AlignText orig, FixVerticalAlign self)
    {
        orig(self);
        if (!string.IsNullOrEmpty(self.transform.parent?.name) && PowerManager.GlobalPowerStates.ContainsKey(self.transform.parent.name.Substring(0, self.transform.parent.name.Length - 7)))
        {
            Text text = self.GetComponent<Text>();
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.lineSpacing = 1f;
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
            LoreMaster.Instance.LogError(exception.Message);
            throw;
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

    #endregion
}
