using LoreMaster.Enums;
using LoreMaster.Helper;
using LoreMaster.LorePowers;
using LoreMaster.Settings;
using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using Modding;
using System;
using System.Collections;
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

    #endregion

    #region Event handler

    private void FixVerticalAlign_AlignText(On.FixVerticalAlign.orig_AlignText orig, FixVerticalAlign self)
    {
        orig(self);
        if (!string.IsNullOrEmpty(self.transform.parent?.name) && Settings.PowerTags.ContainsKey(self.transform.parent.name.Substring(0, self.transform.parent.name.Length - 7)))
        {
            Text text = self.GetComponent<Text>();
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.lineSpacing = 1f;
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

    private void ChangePowerTag(IValueElement element) => Settings.PowerTags[(element as MenuItem<PowerTag>).Name] = (element as MenuItem<PowerTag>).Value;

    private void ChangeEndCondition(BlackEggTempleCondition condition)
    {
        if (condition != BlackEggTempleCondition.Dreamers)
            Elements[5].Show();
        else
            Elements[5].Hide();
    }

    private void ChangedGameMode(GameMode selectedGameMode)
    {
        SettingManager.Instance.GameMode = selectedGameMode;
        if (selectedGameMode != GameMode.Extra)
        {
            Elements[2].Show();
            Elements[3].Show();
        }
        else
        {
            Elements[2].Hide();
            Elements[3].Hide();
        }
    }

    private void MinCursedLore_Changed(int selectedValue)
    {
        int maxValue = ((NumericEntryField<int>)Elements[10]).Value;
        if (selectedValue > maxValue)
        {
            (Elements[9] as NumericEntryField<int>).SetValue(maxValue);
            return;
        }
        int minValue = 1;
        if (Settings.GameMode == GameMode.Hard)
            minValue = 5;
        else if (Settings.GameMode == GameMode.Heroic)
            minValue = 10;

        if (selectedValue < minValue)
            (Elements[9] as NumericEntryField<int>).SetValue(minValue);
    }

    private void MaxCursedLore_Changed(int selectedValue)
    {
        int minValue = ((NumericEntryField<int>)Elements[9]).Value;
        if (selectedValue < minValue)
        {
            (Elements[10] as NumericEntryField<int>).SetValue(minValue);
            return;
        }
        else if (selectedValue > 60)
            (Elements[10] as NumericEntryField<int>).SetValue(60);
    }

    private void CursedLoreButton_ValueChanged(CursedLore option)
    {
        if (option == CursedLore.Fixed)
        {
            Elements[9].Show();
            Elements[10].Show();
        }
        else
        {
            Elements[9].Hide();
            Elements[10].Hide();
            if (option == CursedLore.Random)
            {
                (Elements[9] as NumericEntryField<int>).SetValue(Settings.GameMode == GameMode.Extra ? 1 : (Settings.GameMode == GameMode.Heroic ? 10 : 5));
                (Elements[10] as NumericEntryField<int>).SetValue(Settings.GameMode == GameMode.Extra ? 15 : (Settings.GameMode == GameMode.Heroic ? 40 : 25));
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
            power.Tag = Settings.NightmareMode ? PowerTag.Global : Settings.PowerTags[power.PowerName];
        if (Settings.UseCursedLore != CursedLore.None)
        {
            int minAmount = (Elements[9] as NumericEntryField<int>).Value;
            int maxAmount = (Elements[10] as NumericEntryField<int>).Value;
            int finalAmount = LoreMaster.Instance.Generator.Next(minAmount, maxAmount + 1);
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
        Elements = new IMenuElement[12];
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
        Elements[4] = new MenuItem<BlackEggTempleCondition>(ExtraPage, "End Condition", (BlackEggTempleCondition[])Enum.GetValues(typeof(BlackEggTempleCondition)));
        ((MenuItem<BlackEggTempleCondition>)Elements[4]).ValueChanged += ChangeEndCondition;
        Elements[5] = new NumericEntryField<int>(ExtraPage, "Needed Lore");
        ((NumericEntryField<int>)Elements[5]).Bind(Settings, typeof(ExtraSettings).GetProperty("NeededLore", BindingFlags.Public | BindingFlags.Instance));
        Elements[5].MoveTo(((NumericEntryField<int>)Elements[5]).GameObject.transform.position - new Vector3(0f, -50f));
        Elements[5].Hide();

        // Steel soul option
        Elements[6] = new ToggleButton(ExtraPage, "Steel Soul");
        ((ToggleButton)Elements[6]).Bind(Settings, typeof(ExtraSettings).GetProperty("SteelSoul", BindingFlags.Public | BindingFlags.Instance));
        Elements[7] = new SmallButton(ExtraPage, "Power Tags");
        ((SmallButton)Elements[7]).AddHideAndShowEvent(ExtraPage, PowerPage);
        
        // Power tag control
        PowerElements = new MenuItem<PowerTag>[60];
        List<PowerTag> tags = (Enum.GetValues(typeof(PowerTag)) as PowerTag[]).ToList();
        int index = 0;
        foreach (string key in Settings.PowerTags.Keys)
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
            item.SetValue(Settings.PowerTags[item.Name]);

        // Cursed lore stuff
        MenuEnum<CursedLore> cursedLoreButton = new(ExtraPage, "Cursed Lore");
        cursedLoreButton.ValueChanged += CursedLoreButton_ValueChanged;
        Elements[8] = cursedLoreButton;
        cursedLoreButton.Bind(Settings, typeof(ExtraSettings).GetProperty("UseCursedLore", BindingFlags.Public | BindingFlags.Instance));

        NumericEntryField<int> minCursedLore = new(ExtraPage, "Min. cursed Lore");
        minCursedLore.ValueChanged += MinCursedLore_Changed;
        Elements[9] = minCursedLore;
        minCursedLore.Bind(Settings, typeof(ExtraSettings).GetProperty("MinCursedLore", BindingFlags.Public | BindingFlags.Instance));

        NumericEntryField<int> maxCursedLore = new(ExtraPage, "Max. cursed Lore");
        maxCursedLore.ValueChanged += MaxCursedLore_Changed;
        Elements[10] = maxCursedLore;
        maxCursedLore.Bind(Settings, typeof(ExtraSettings).GetProperty("MaxCursedLore", BindingFlags.Public | BindingFlags.Instance));

        // Start button
        BigButton startButton = new(ExtraPage, "Start Game");
        startButton.OnClick += StartGame;
        Elements[11] = startButton;

        // Create main page
        new VerticalItemPanel(ExtraPage, new(0f, 400f), 60, false, Elements);
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
