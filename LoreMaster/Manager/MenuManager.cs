using LoreMaster.Enums;
using LoreMaster.Helper;
using LoreMaster.LorePowers;
using LoreMaster.Settings;
using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace LoreMaster.Manager;

internal class MenuManager : ModeMenuConstructor
{
    #region Properties

    public MenuPage ExtraPage { get; set; }

    public IMenuElement[] Elements { get; set; }

    public MenuPage PowerPage { get; set; }

    public MenuItem<PowerTag>[] PowerElements { get; set; }

    public GridItemPanel FirstPowerSet { get; set; }

    public GridItemPanel SecondPowerSet { get; set; }

    public SmallButton[] MoveButtons { get; set; }

    public ExtraSettings Settings { get; set; } = new();

    public NumericEntryField<int> NeededLore { get; set; }

    public GridItemPanel CursedRange { get; set; }

    #endregion

    #region Event handler

    private void FixVerticalAlign_AlignText(On.FixVerticalAlign.orig_AlignText orig, FixVerticalAlign self)
    {
        orig(self);

        try
        {
            if (!string.IsNullOrEmpty(self.transform.parent?.name) && PowerManager.GlobalPowerStates.ContainsKey(self.transform.parent.name.Substring(0, self.transform.parent.name.Length - 7)))
            {
                Text text = self.GetComponent<Text>();
                text.verticalOverflow = VerticalWrapMode.Overflow;
                text.lineSpacing = 1f;
            }
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogWarn("An exception occured while modifying text alignment: " + exception.Message);
        }
    }

    private void LeftButton_OnClick()
    {
        FirstPowerSet.Show();
        SecondPowerSet.Hide();
        MoveButtons[0].Show();
        MoveButtons[1].Hide();
    }

    private void RightButton_OnClick()
    {
        FirstPowerSet.Hide();
        SecondPowerSet.Show();
        MoveButtons[1].Show();
        MoveButtons[0].Hide();
    }

    private void Change_PowerControl(PowerTag chosenTag)
    {
        foreach (MenuItem<PowerTag> item in PowerElements)
            item.SetValue(chosenTag);
    }

    private void ChangePowerTag(IValueElement element) => PowerManager.GlobalPowerStates[(element as MenuItem<PowerTag>).Name] = (element as MenuItem<PowerTag>).Value;

    private void ChangeEndCondition(BlackEggTempleCondition condition)
    {
        if (condition != BlackEggTempleCondition.Dreamers)
            NeededLore.Show();
        else
            NeededLore.Hide();
        Settings.EndCondition = condition;
    }

    private void ChangedGameMode(GameMode selectedGameMode)
    {
        SettingManager.Instance.GameMode = selectedGameMode;
        if (selectedGameMode != GameMode.Extra)
        {
            Elements[2].Show();
            Elements[3].Show();
            int minValue = selectedGameMode == GameMode.Extra
                    ? 1
                    : selectedGameMode == GameMode.Hard
                        ? 5
                        : 10;
            int maxValue = selectedGameMode == GameMode.Extra
                ? 15
                : selectedGameMode == GameMode.Hard
                    ? 20
                    : 25;
            if (Settings.MinCursedLore < minValue)
                (CursedRange.Items[0] as NumericEntryField<int>).SetValue(minValue);
            if (Settings.MaxCursedLore < maxValue)
                (CursedRange.Items[1] as NumericEntryField<int>).SetValue(maxValue);
        }
        else
        {
            Elements[2].Hide();
            Elements[3].Hide();
            (Elements[3] as ToggleButton).SetValue(false);
        }
    }

    private void MinCursedLore_Changed(int selectedValue)
    {
        if (selectedValue < 0)
            (CursedRange.Items[0] as NumericEntryField<int>).SetValue(0);
        else if (selectedValue > Settings.MaxCursedLore)
            (CursedRange.Items[0] as NumericEntryField<int>).SetValue(Settings.MaxCursedLore);
        else
        {
            int minValue = Settings.GameMode == GameMode.Extra
                    ? 1
                    : Settings.GameMode == GameMode.Hard
                        ? 5
                        : 10;
            if (selectedValue < minValue)
            {
                if (Settings.MaxCursedLore < minValue)
                    (CursedRange.Items[1] as NumericEntryField<int>).SetValue(minValue);
                (CursedRange.Items[0] as NumericEntryField<int>).SetValue(minValue);
            }
        }
    }

    private void MaxCursedLore_Changed(int selectedValue)
    {
        if (selectedValue > 60)
            (CursedRange.Items[1] as NumericEntryField<int>).SetValue(60);
        else if (selectedValue < Settings.MinCursedLore)
            (CursedRange.Items[1] as NumericEntryField<int>).SetValue(Settings.MinCursedLore);
        else
        {
            int maxValue = Settings.GameMode == GameMode.Extra
                    ? 15
                    : Settings.GameMode == GameMode.Hard
                        ? 20
                        : 25;
            if (selectedValue < maxValue)
            {
                if (maxValue < Settings.MinCursedLore)
                    (CursedRange.Items[1] as NumericEntryField<int>).SetValue(Settings.MinCursedLore);
                else
                    (CursedRange.Items[1] as NumericEntryField<int>).SetValue(maxValue);
            }
        }
    }

    private void CursedLoreButton_ValueChanged(CursedLore option)
    {
        if (option == CursedLore.Custom)
            CursedRange.Show();
        else
        {
            CursedRange.Hide();
            if (option == CursedLore.Random)
            {
                int minValue = Settings.GameMode == GameMode.Extra
                    ? 1
                    : Settings.GameMode == GameMode.Hard
                        ? 5
                        : 10;
                int maxValue = Settings.GameMode == GameMode.Extra
                    ? 15
                    : Settings.GameMode == GameMode.Hard
                        ? 20
                        : 25;
                if (maxValue < Settings.MinCursedLore)
                    (CursedRange.Items[1] as NumericEntryField<int>).SetValue(Settings.MinCursedLore);
                else
                {
                    (CursedRange.Items[1] as NumericEntryField<int>).SetValue(maxValue);
                    if (minValue > Settings.MinCursedLore)
                        (CursedRange.Items[0] as NumericEntryField<int>).SetValue(minValue);
                }
            }
        }
    }

    private void StartGame()
    {
        // Parse options
        SettingManager.Instance.EndCondition = Settings.EndCondition;
        SettingManager.Instance.NeededLore = Settings.NeededLore;
        SettingManager.Instance.GameMode = Settings.GameMode;
        foreach (Power power in PowerManager.GetAllPowers())
            power.Tag = Settings.NightmareMode ? PowerTag.Global : PowerManager.GlobalPowerStates[power.PowerName];
        if (Settings.UseCursedLore != CursedLore.None)
        {
            int finalAmount = LoreMaster.Instance.Generator.Next(Settings.MinCursedLore, Settings.MaxCursedLore + 1);
            List<Power> lorePowers = PowerManager.GetAllPowers().ToList();
            for (int i = 0; i < finalAmount; i++)
            {
                int rolledLore = LoreMaster.Instance.Generator.Next(0, lorePowers.Count);
                lorePowers[rolledLore].StayTwisted = true;
                lorePowers.RemoveAt(rolledLore);
            }
            // Reset all none selected.
            foreach (Power power in lorePowers)
                power.StayTwisted = false;
        }
        else
            foreach (Power power in PowerManager.GetAllPowers())
                power.StayTwisted = false;
        
        LoreManager.Instance.CanRead = false;
        LoreManager.Instance.CanListen = false;
        UIManager.instance.StartNewGame(Settings.SteelSoul);
    }

    #endregion

    #region Methods

    internal static void AddMode() => ModeMenu.AddMode(new MenuManager());

    public override void OnEnterMainMenu(MenuPage modeMenu)
    {
        ExtraPage = new("Lore Master Extra", modeMenu);
        PowerPage = new("Power Tags", ExtraPage);
        Elements = new IMenuElement[6];
        Elements[0] = new MenuLabel(ExtraPage, "Lore Master Extra");
        Elements[1] = new MenuItem<GameMode>(ExtraPage, "Difficulty", new GameMode[] { GameMode.Extra, GameMode.Hard, GameMode.Heroic });
        ((MenuItem<GameMode>)Elements[1]).Bind(Settings, typeof(ExtraSettings).GetProperty("GameMode", BindingFlags.Public | BindingFlags.Instance));
        ((MenuItem<GameMode>)Elements[1]).ValueChanged += ChangedGameMode;

        // Nightmare difficulty settings.
        Elements[2] = new MenuLabel(ExtraPage, "Nightmare mode is extremely hard/unfair and was never intended to actually be beatable. Be aware of that.", MenuLabel.Style.Body);
        Elements[3] = new ToggleButton(ExtraPage, "Nightmare Mode");
        ((ToggleButton)Elements[3]).Bind(Settings, typeof(ExtraSettings).GetProperty("NightmareMode", BindingFlags.Public | BindingFlags.Instance));
        Elements[2].Hide();
        Elements[3].Hide();

        // End condition
        MenuItem<BlackEggTempleCondition> endCondition = new(ExtraPage, "End Condition", (BlackEggTempleCondition[])Enum.GetValues(typeof(BlackEggTempleCondition)));
        endCondition.ValueChanged += ChangeEndCondition;
        endCondition.MoveTo(new(-500f, 0f));
        NeededLore = new(ExtraPage, "Needed Lore");
        NeededLore.Bind(Settings, typeof(ExtraSettings).GetProperty("NeededLore", BindingFlags.Public | BindingFlags.Instance));
        NeededLore.ValueChanged += NeededLore_ValueChanged;
        NeededLore.MoveTo(new(-500f, -150f));
        NeededLore.Hide();

        // Steel soul option
        Elements[4] = new ToggleButton(ExtraPage, "Steel Soul");
        ((ToggleButton)Elements[4]).Bind(Settings, typeof(ExtraSettings).GetProperty("SteelSoul", BindingFlags.Public | BindingFlags.Instance));
        Elements[5] = new SmallButton(ExtraPage, "Power Tags");
        ((SmallButton)Elements[5]).AddHideAndShowEvent(ExtraPage, PowerPage);

        // Power tag control
        PowerElements = new MenuItem<PowerTag>[60];
        List<PowerTag> tags = (Enum.GetValues(typeof(PowerTag)) as PowerTag[]).ToList();
        int index = 0;
        if (PowerManager.GlobalPowerStates == null)
        {
            PowerManager.GlobalPowerStates = new();
            foreach (Power power in PowerManager.GetAllPowers())
                PowerManager.GlobalPowerStates.Add(power.PowerName, power.Tag);
        }
        foreach (string key in PowerManager.GlobalPowerStates.Keys)
        {
            MenuItem<PowerTag> item = new(PowerPage, key, tags, new MultiLineFormatter());
            // This is a wacky workaround, since I can't bind on a dictionary that easily and have no clue how I'd implement that.
            item.SelfChanged += ChangePowerTag;
            PowerElements[index] = item;
            index++;
        }

        MenuItem<PowerTag> powerControl = new(PowerPage, "All Powers", tags);
        powerControl.ValueChanged += Change_PowerControl;
        MoveButtons = new SmallButton[2];
        SmallButton rightButton = new(PowerPage, ">>");
        rightButton.OnClick += RightButton_OnClick;
        MoveButtons[0] = rightButton;

        SmallButton leftButton = new(PowerPage, "<<");
        leftButton.OnClick += LeftButton_OnClick;
        MoveButtons[1] = leftButton;
        powerControl.MoveTo(new(0, 440f));
        leftButton.MoveTo(new(-700, -430));
        rightButton.MoveTo(new(700, -430));
        FirstPowerSet = new GridItemPanel(PowerPage, new Vector2(0, 370), 5, 150, 370, false, PowerElements.Take(30).ToArray());
        SecondPowerSet = new GridItemPanel(PowerPage, new Vector2(0, 370), 5, 150, 370, false, PowerElements.Skip(30).ToArray());
        SecondPowerSet.Hide();
        leftButton.Hide();
        On.FixVerticalAlign.AlignText += FixVerticalAlign_AlignText;
        foreach (MenuItem<PowerTag> item in PowerElements)
            item.SetValue(PowerManager.GlobalPowerStates[item.Name]);

        // Cursed lore stuff
        MenuEnum<CursedLore> cursedLoreButton = new(ExtraPage, "Cursed Lore");
        cursedLoreButton.ValueChanged += CursedLoreButton_ValueChanged;
        cursedLoreButton.Bind(Settings, typeof(ExtraSettings).GetProperty("UseCursedLore", BindingFlags.Public | BindingFlags.Instance));
        cursedLoreButton.MoveTo(new(500f, 0f));

        NumericEntryField<int> minCursedLore = new(ExtraPage, "Min. cursed Lore");
        minCursedLore.ValueChanged += MinCursedLore_Changed;
        minCursedLore.Bind(Settings, typeof(ExtraSettings).GetProperty("MinCursedLore", BindingFlags.Public | BindingFlags.Instance));

        NumericEntryField<int> maxCursedLore = new(ExtraPage, "Max. cursed Lore");
        maxCursedLore.ValueChanged += MaxCursedLore_Changed;
        maxCursedLore.Bind(Settings, typeof(ExtraSettings).GetProperty("MaxCursedLore", BindingFlags.Public | BindingFlags.Instance));

        // Start button
        BigButton startButton = new(ExtraPage, "Start Game");
        startButton.OnClick += StartGame;
        startButton.MoveTo(new(0f, -300f));

        CursedRange = new(ExtraPage, new(500, -150f), 2, 600, 230, false, new IMenuElement[] { minCursedLore, maxCursedLore });
        CursedRange.Hide();

        // Create main page
        new VerticalItemPanel(ExtraPage, new(0f, 400f), 80, false, Elements);
    }

    private void NeededLore_ValueChanged(int obj)
    {
        if (obj < 0)
            NeededLore.SetValue(0);
        else if (obj > 60)
            NeededLore.SetValue(60);
    }

    public override void OnExitMainMenu()
    {
        Elements = null;
        PowerElements = null;
        MoveButtons = null;
        ExtraPage = null;
        FirstPowerSet = null;
        SecondPowerSet = null;
        PowerPage = null;
        On.FixVerticalAlign.AlignText -= FixVerticalAlign_AlignText;
    }

    public override bool TryGetModeButton(MenuPage modeMenu, out BigButton button)
    {
        button = new BigButton(modeMenu, SpriteHelper.CreateSprite("Lore"), "Lore Master Extra");
        button.AddHideAndShowEvent(modeMenu, ExtraPage);
        return true;
    }

    #endregion
}

internal class MultiLineFormatter : MenuItemFormatter
{
    public override string GetText(string prefix, object value) => $"{prefix}\n{value}";
}
