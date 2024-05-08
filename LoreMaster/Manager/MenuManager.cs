//using KorzUtils.Helper;
//using LoreMaster.Enums;
//using LoreMaster.Settings;
//using MenuChanger;
//using MenuChanger.Extensions;
//using MenuChanger.MenuElements;
//using MenuChanger.MenuPanels;
//using System;
//using System.Reflection;

//namespace LoreMaster.Manager;

//internal class MenuManager : ModeMenuConstructor
//{
//    #region Properties

//    public MenuPage ExtraPage { get; set; }

//    public IMenuElement[] Elements { get; set; }

//    public MenuPage PowerPage { get; set; }

//    public MenuItem<PowerRank>[] PowerElements { get; set; }

//    public GridItemPanel FirstPowerSet { get; set; }

//    public GridItemPanel SecondPowerSet { get; set; }

//    public SmallButton[] MoveButtons { get; set; }

//    public ExtraSettings Settings { get; set; } = new();

//    public NumericEntryField<int> NeededLore { get; set; }

//    public GridItemPanel CursedRange { get; set; }

//    #endregion

//    #region Event handler

//    private void ChangeEndCondition(BlackEggTempleCondition condition)
//    {
//        if (condition != BlackEggTempleCondition.Dreamers)
//            NeededLore.Show();
//        else
//            NeededLore.Hide();
//        Settings.EndCondition = condition;
//    }

//    private void ChangedGameMode(GameMode selectedGameMode)
//    {
//        if (selectedGameMode != GameMode.Extra)
//        {
//            Elements[2].Show();
//            Elements[3].Show();
//            int minValue = selectedGameMode == GameMode.Extra
//                    ? 1
//                    : selectedGameMode == GameMode.Hard
//                        ? 5
//                        : 10;
//            int maxValue = selectedGameMode == GameMode.Extra
//                ? 15
//                : selectedGameMode == GameMode.Hard
//                    ? 20
//                    : 25;
//        }
//        else
//        {
//            Elements[2].Hide();
//            Elements[3].Hide();
//            (Elements[3] as ToggleButton).SetValue(false);
//        }
//    }

//    private void StartGame()
//    {
//        UIManager.instance.StartNewGame(Settings.SteelSoul);
//    }

//    #endregion

//    #region Methods

//    internal static void AddMode() => ModeMenu.AddMode(new MenuManager());

//    public override void OnEnterMainMenu(MenuPage modeMenu)
//    {
//        ExtraPage = new("Lore Master Extra", modeMenu);
//        PowerPage = new("Power Tags", ExtraPage);
//        Elements = new IMenuElement[5];
//        Elements[0] = new MenuLabel(ExtraPage, "Lore Master Extra");
//        Elements[1] = new MenuItem<GameMode>(ExtraPage, "Difficulty", new GameMode[] { GameMode.Extra, GameMode.Hard, GameMode.Heroic });
//        ((MenuItem<GameMode>)Elements[1]).Bind(Settings, typeof(ExtraSettings).GetProperty("GameMode", BindingFlags.Public | BindingFlags.Instance));
//        ((MenuItem<GameMode>)Elements[1]).ValueChanged += ChangedGameMode;

//        // Nightmare difficulty settings.
//        Elements[2] = new MenuLabel(ExtraPage, "Nightmare mode is extremely hard/unfair and was never intended to actually be beatable. Be aware of that.", MenuLabel.Style.Body);
//        Elements[3] = new ToggleButton(ExtraPage, "Nightmare Mode");
//        ((ToggleButton)Elements[3]).Bind(Settings, typeof(ExtraSettings).GetProperty("NightmareMode", BindingFlags.Public | BindingFlags.Instance));
//        Elements[2].Hide();
//        Elements[3].Hide();

//        // End condition
//        MenuItem<BlackEggTempleCondition> endCondition = new(ExtraPage, "End Condition", (BlackEggTempleCondition[])Enum.GetValues(typeof(BlackEggTempleCondition)));
//        endCondition.ValueChanged += ChangeEndCondition;
//        endCondition.MoveTo(new(-500f, 0f));
//        NeededLore = new(ExtraPage, "Needed Lore");
//        NeededLore.Bind(Settings, typeof(ExtraSettings).GetProperty("NeededLore", BindingFlags.Public | BindingFlags.Instance));
//        NeededLore.ValueChanged += NeededLore_ValueChanged;
//        NeededLore.MoveTo(new(-500f, -150f));
//        NeededLore.Hide();

//        // Steel soul option
//        Elements[4] = new ToggleButton(ExtraPage, "Steel Soul");
//        ((ToggleButton)Elements[4]).Bind(Settings, typeof(ExtraSettings).GetProperty("SteelSoul", BindingFlags.Public | BindingFlags.Instance));

//        // Start button
//        BigButton startButton = new(ExtraPage, "Start Game");
//        startButton.OnClick += StartGame;
//        startButton.MoveTo(new(0f, -300f));

//        // Create main page
//        new VerticalItemPanel(ExtraPage, new(0f, 400f), 80, false, Elements);
//    }

//    private void NeededLore_ValueChanged(int obj)
//    {
//        if (obj < 0)
//            NeededLore.SetValue(0);
//        else if (obj > 60)
//            NeededLore.SetValue(60);
//    }

//    public override void OnExitMainMenu()
//    {
//        Elements = null;
//        PowerElements = null;
//        MoveButtons = null;
//        ExtraPage = null;
//        FirstPowerSet = null;
//        SecondPowerSet = null;
//        PowerPage = null;
//    }

//    public override bool TryGetModeButton(MenuPage modeMenu, out BigButton button)
//    {
//        button = new BigButton(modeMenu, SpriteHelper.CreateSprite<LoreMaster>("Sprites.Lore"), "Lore Master Extra");
//        button.AddHideAndShowEvent(modeMenu, ExtraPage);
//        return true;
//    }

//    #endregion
//}

//internal class MultiLineFormatter : MenuItemFormatter
//{
//    public override string GetText(string prefix, object value) => $"{prefix}\n{value}";
//}
