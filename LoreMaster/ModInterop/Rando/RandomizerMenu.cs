using KorzUtils.Helper;
using LoreMaster.Settings;
using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using RandomizerMod.Menu;
using System;

namespace LoreMaster.ModInterop.Rando;

public class RandomizerMenu
{
    #region Members

    private static RandomizerMenu _instance;

    private MenuPage _mainPage;

    private SmallButton _pageButton;

    private MenuElementFactory<RandoSettings> _menuElementFactory;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the instance of the menu.
    /// </summary>
    public static RandomizerMenu Instance => _instance ??= new();

    #endregion

    #region Setup

    public static void AttachMenu()
    {
        RandomizerMenuAPI.AddMenuPage(Instance.ConstructMenu, Instance.HandleButton);
        MenuChangerMod.OnExitMainMenu += () => _instance = null;
    }

    private bool HandleButton(MenuPage previousPage, out SmallButton button)
    {
        try
        {
            _pageButton = new(previousPage, "LoreMaster");
            _pageButton.AddHideAndShowEvent(previousPage, _mainPage);
            _mainPage.BeforeGoBack += () => _pageButton.Text.color = !LoreMaster.Instance.RandomizerSettings.Enabled ? Colors.DEFAULT_COLOR : Colors.TRUE_COLOR;
            _pageButton.Text.color = !LoreMaster.Instance.RandomizerSettings.Enabled ? Colors.DEFAULT_COLOR : Colors.TRUE_COLOR;
            button = _pageButton;
            return true;
        }
        catch (Exception error)
        {
            LogHelper.Write<LoreMaster>("Failed to create connection button: ", error, false);
        }
        button = null;
        return true;
    }

    private void ConstructMenu(MenuPage previousPage)
    {
        try
        {
            _mainPage = new("LoreMaster", previousPage);
            _menuElementFactory = new(_mainPage, LoreMaster.Instance.RandomizerSettings);
            new VerticalItemPanel(_mainPage, new(0f, 450f), 80f, true, _menuElementFactory.Elements);
        }
        catch (Exception exception)
        {
            LogHelper.Write<LoreMaster>("Failed to construct connection menu: ", exception, false);
        }
    }

    #endregion

    #region Interop

    internal void PassRSMSettings(RandoSettings settings)
    {
        if (settings == null)
            _menuElementFactory.ElementLookup[nameof(LoreMaster.Instance.RandomizerSettings.Enabled)].SetValue(false);
        else
            _menuElementFactory.SetMenuValues(settings);
    }

    #endregion
}
