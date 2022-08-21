using LoreMaster.Enums;
using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using RandomizerMod.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.Randomizer;

/// <summary>
/// Menu page for the rando settings.
/// </summary>
public class RandomizerMenu
{
    #region Members

    internal MenuPage _mainPage;

    internal VerticalItemPanel _optionPanel;

    internal MenuElementFactory<RandomizerSettings> _menuElementFactory;

    internal SmallButton _pageButton;

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
            if (RandomizerManager.Settings.RandomizeWarriorStatues)
                maxLore += 7;
            return maxLore;
        }
    }

    #endregion

    #region Event handler

    /// <summary>
    /// Event handler when the main menu is closed.
    /// </summary>
    public static void OnExitMenu() => _instance = null;

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
            _mainPage = new MenuPage("Lore Master", previousPage);
            _menuElementFactory = new(_mainPage, RandomizerManager.Settings);
            _menuElementFactory.Elements[0].SelfChanged += AdjustLoreCap;
            _menuElementFactory.Elements[1].SelfChanged += AdjustLoreCap;
            // Add event handler to show/hide the needed lore option depending on the end condition option.
            _menuElementFactory.Elements[5].SelfChanged += ChangeCondition;
            if (RandomizerManager.Settings.BlackEggTempleCondition == RandomizerEndCondition.Dreamers)
                _menuElementFactory.Elements[6].Hide();
            _menuElementFactory.Elements[6].SelfChanged += ChangeLoreAmount;
            _optionPanel = new(_mainPage, new(0, 300), 80f, true, _menuElementFactory.Elements);
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Error in construct menu: " + exception.Message);
        }
    }

    /// <summary>
    /// Adjust the lore cap.
    /// </summary>
    private void AdjustLoreCap(IValueElement npc)
    {
        if ((int)_menuElementFactory.Elements[6].Value > MaxLore)
            _menuElementFactory.Elements[6].SetValue(MaxLore);
    }

    /// <summary>
    /// Changes the lore amount in the range.
    /// </summary>
    /// <param name="loreAmount"></param>
    private void ChangeLoreAmount(IValueElement loreAmount)
    {
        if ((int)loreAmount.Value < 1)
            loreAmount.SetValue(1);
        else if ((int)loreAmount.Value > (MaxLore))
            loreAmount.SetValue(MaxLore);
    }

    /// <summary>
    /// Change the temple condition and show the amount value.
    /// </summary>
    /// <param name="endCondition"></param>
    private void ChangeCondition(IValueElement endCondition)
    {
        if ((RandomizerEndCondition)endCondition.Value != RandomizerEndCondition.Dreamers)
            _menuElementFactory.Elements[6].Show();
        else
            _menuElementFactory.Elements[6].Hide();
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

    #endregion
}
