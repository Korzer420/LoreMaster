using LoreMaster.Enums;
using LoreMaster.Helper;
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
    public MenuPage ExtraPage { get; set; }

    public MenuPage PowerPage { get; set; }

    public IMenuElement[] Elements { get; set; }

    public MenuItem<PowerTag>[] SubElements { get; set; }

    public ExtraSettings Settings { get; set; } = new();

    internal static void AddMode() => ModeMenu.AddMode(new MenuManager());

    public override void OnEnterMainMenu(MenuPage modeMenu)
    {
        ExtraPage = new("Lore Master Extra", modeMenu);
        PowerPage = new("Power Tags", ExtraPage);
        Elements = new IMenuElement[8];
        Elements[0] = new MenuLabel(ExtraPage, "Lore Master Extra");
        Elements[1] = new MenuItem<GameMode>(ExtraPage, "Difficulty", new GameMode[] {GameMode.Extra, GameMode.Hard, GameMode.Heroic});
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
        Elements[5].Hide();

        // Steel soul option
        Elements[6] = new ToggleButton(ExtraPage, "Steel Soul");
        ((ToggleButton)Elements[6]).Bind(Settings, typeof(ExtraSettings).GetProperty("SteelSoul", BindingFlags.Public | BindingFlags.Instance));
        Elements[7] = new SmallButton(ExtraPage, "Power Tags");
        ((SmallButton)Elements[7]).AddHideAndShowEvent(ExtraPage, PowerPage);
        new VerticalItemPanel(ExtraPage, new(0f, 400f), 60, false, Elements);

        SubElements = new MenuItem<PowerTag>[30];
        List<PowerTag> tags = (Enum.GetValues(typeof(PowerTag)) as PowerTag[]).ToList();
        int index = 0;
        foreach (string key in Settings.PowerTags.Keys.Take(30))
        {
            MenuItem<PowerTag> item = new(PowerPage, key, tags, new MultiLineFormatter());
            // This is a wacky workaround, since I can't bind on a dictionary that easily and have no clue how I'd implement that.
            item.SelfChanged += ChangePowerTag;
            SubElements[index] = item;
            Component.Destroy(item.GameObject.GetComponentInChildren<FixVerticalAlign>());
            Text text = item.GameObject.transform.Find("Text").GetComponent<Text>();
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.lineSpacing = 1f;
            index++;
        }
        new GridItemPanel(PowerPage, new Vector2(0, 400), 5, 200, 300, false, SubElements);

    }

    private void ChangePowerTag(IValueElement obj) => Settings.PowerTags[(obj as MenuItem<PowerTag>).Name] = (obj as MenuItem<PowerTag>).Value;
    
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
    
    public override void OnExitMainMenu()
    {
        Elements = null;
        ExtraPage = null;
    }

    public override bool TryGetModeButton(MenuPage modeMenu, out BigButton button)
    {
        button = new BigButton(modeMenu, SpriteHelper.CreateSprite("Lore"), "Lore Master Extra");
        button.AddHideAndShowEvent(modeMenu, ExtraPage);
        return true;
    }
}

internal class MultiLineFormatter : MenuItemFormatter
{
    public override string GetText(string prefix, object value) => $"{prefix}\n{value}";
}
