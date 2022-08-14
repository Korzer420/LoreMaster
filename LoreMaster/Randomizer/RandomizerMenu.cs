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
            // Add event handler to show/hide the needed lore option depending on the end condition option.
            _menuElementFactory.Elements[3].SelfChanged += ChangeCondition;
            if (RandomizerManager.Settings.TempleCondition == RandomizerEndCondition.Dreamers)
                _menuElementFactory.Elements[4].Hide();
            _menuElementFactory.Elements[4].SelfChanged += ChangeLoreAmount;
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
        if (!(bool)npc.Value && (int)_menuElementFactory.Elements[4].Value > 30)
            _menuElementFactory.Elements[4].SetValue(30);
    }

    /// <summary>
    /// Changes the lore amount in the range.
    /// </summary>
    /// <param name="loreAmount"></param>
    private void ChangeLoreAmount(IValueElement loreAmount)
    {
        if ((int)loreAmount.Value < 1)
            loreAmount.SetValue(1);
        else if ((int)loreAmount.Value > (RandomizerManager.Settings.RandomizeNpc ? 48 : 30))
            loreAmount.SetValue(RandomizerManager.Settings.RandomizeNpc ? 48 : 30);
    }

    /// <summary>
    /// Change the temple condition and show the amount value.
    /// </summary>
    /// <param name="endCondition"></param>
    private void ChangeCondition(IValueElement endCondition)
    {
        if ((RandomizerEndCondition)endCondition.Value != RandomizerEndCondition.Dreamers)
            _menuElementFactory.Elements[4].Show();
        else
            _menuElementFactory.Elements[4].Hide();
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
