using ItemChanger;
using ItemChanger.Components;
using ItemChanger.Items;
using ItemChanger.Locations;
using ItemChanger.Placements;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using LoreMaster.ItemChanger;
using LoreMaster.ItemChanger.Locations;
using LoreMaster.LorePowers.CityOfTears;
using LoreMaster.Randomizer.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LoreMaster.Manager;

public static class ItemManager
{
    #region Constants

    #region Location Names

    public const string City_Teleporter = "City_Teleporter";

    public const string Temple_Teleporter = "Temple_Teleporter";

    public const string Lemm_Door = "Lemm_Door";

    public const string Iselda_Treasure = "Iselda_Treasure";

    public const string Treasure_Prefix = "Treasure_";

    public const string Stag_Nest = "Stag_Nest";

    public const string Path_of_Pain_End_Scene = "Path_of_Pain-End_Scene";

    public const string Record_Bela = "Record_Bela";

    public const string Zote_Deepnest = "Zote_Deepnest";

    public const string Elderbug_Reward_Prefix = "Elderbug_Reward_";

    #endregion

    #region Item Names

    public const string City_Ticket = "City_Ticket";

    public const string Temple_Ticket = "Temple_Ticket";

    public const string Lemm_Order = "Lemm_Order";

    public const string Lemm_Sign = "Lore_Tablet-Lemm_Sign";

    public const string Treasure_Chart_Prefix = "Treasure_Chart_";

    public const string Silksong_Journal = "Silksong_Journal";

    public const string Silver_Hallownest_Seal = "Silver_Hallownest_Seal";

    public const string Bronze_King_Idol = "Bronze_King_Idol";

    public const string Golden_Arcane_Egg = "Golden_Arcane_Egg";

    public const string Magical_Key = "Magical_Key";

    public const string Dream_Medallion = "Dream_Medallion";

    public const string Stag_Egg_Inspect = "Lore_Tablet-Stag_Egg_Inspect";

    public const string Stag_Egg = "Stag_Egg";

    public const string Path_of_Pain_Reward = "Lore_Tablet-Path_of_Pain_Reward";

    public const string Sanctum_Spell_Twister = "Lore_Tablet-Sanctum_Spell_Twister";

    public const string Read_Ability = "Read_Ability";

    public const string Listen_Ability = "Listen_Ability";

    public const string Lore_Page = "Lore_Page";

    public const string Joker_Scroll = "Joker_Scroll";

    public const string Lore_Page_Control = "Lore_Page_Control";

    public const string Cleansing_Scroll = "Cleansing_Scroll";

    public const string Cleansing_Scroll_Double = "Cleansing_Scroll_Double";

    #endregion

    #endregion

    /// <summary>
    /// Defines all locations and items that this mod uses.
    /// </summary>
    internal static void DefineIC()
    {
        ItemChangerMod.CreateSettingsProfile(false);

        try
        {
            DefineTeleporter();
            DefineTreasure();
            DefineStagEgg();
            DefineExtraLore();
            DefineElderbug();
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Couldn't define items/locations: " + exception.Message);
            LoreMaster.Instance.LogError(exception.StackTrace);
        }
    }

    /// <summary>
    /// Adds the custom placements to the save file.
    /// </summary>
    internal static void CreatePlacements()
    {
        ItemChangerMod.CreateSettingsProfile(false);
        List<AbstractPlacement> placements = new();

        try
        {
            placements.AddRange(CreateTeleporter());
            placements.AddRange(CreateTreasure());
            placements.Add(CreateStagEgg());
            placements.AddRange(CreateExtraLore());
            placements.AddRange(CreateElderbugRewards());
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Couldn't create placements: " + exception.Message);
            LoreMaster.Instance.LogError(exception.StackTrace);
        }
        ItemChangerMod.AddPlacements(placements);
    }

    internal static T GetLocationByName<T>(string name) where T : AbstractLocation => Finder.GetLocation(name) as T;

    #region Generating

    private static void DefineTeleporter()
    {
        Finder.DefineCustomLocation(new CoordinateLocation() { x = 35.0f, y = 5.4f, elevation = 0, sceneName = "Ruins1_27", name = City_Teleporter });
        Finder.DefineCustomLocation(new CoordinateLocation() { x = 57f, y = 5f, elevation = 0, sceneName = "Room_temple", name = Temple_Teleporter });
        Finder.DefineCustomItem(new TouristMagnetItem()
        {
            name = City_Ticket,
            UIDef = new MsgUIDef()
            {
                name = new BoxedString("Lumafly Express"),
                shopDesc = new BoxedString("If you see this, something went wrong."),
                sprite = (Finder.GetItem(ItemNames.Tram_Pass).GetResolvedUIDef() as MsgUIDef).sprite
            },
            tags = new List<Tag>()
            {
                new PersistentItemTag() { Persistence = Persistence.Persistent},
                new CompletionWeightTag() { Weight = 0}
            }
        });
        Finder.DefineCustomItem(new TouristMagnetItem()
        {
            name = Temple_Ticket,
            UIDef = new MsgUIDef()
            {
                name = new BoxedString("Lumafly Express"),
                shopDesc = new BoxedString("If you see this, something went wrong."),
                sprite = (Finder.GetItem(ItemNames.Tram_Pass).GetResolvedUIDef() as MsgUIDef).sprite
            },
            tags = new List<Tag>()
            {
                new PersistentItemTag() { Persistence = Persistence.Persistent},
                new CompletionWeightTag() { Weight = 0}
            }
        });
    }

    private static List<AbstractPlacement> CreateTeleporter()
    {
        List<AbstractPlacement> teleporter = new();
        MutablePlacement placement = new(City_Teleporter);
        placement.Location = Finder.GetLocation(City_Teleporter) as CoordinateLocation;
        placement.Cost = new Paypal() { ToTemple = true };
        placement.containerType = Container.Shiny;
        placement.Add(Finder.GetItem(City_Ticket));
        teleporter.Add(placement);
        placement = new(Temple_Teleporter);
        placement.Location = Finder.GetLocation(Temple_Teleporter) as CoordinateLocation;
        placement.Cost = new Paypal() { ToTemple = false };
        placement.containerType = Container.Shiny;
        placement.Add(Finder.GetItem(Temple_Ticket));
        teleporter.Add(placement);
        return teleporter;
    }

    private static void DefineTreasure()
    {
        // Lemms door
        Finder.DefineCustomLocation(new LemmSignLocation()
        {
            sceneName = "Ruins1_05b",
            name = Lemm_Door,
            flingType = FlingType.DirectDeposit
        });
        Finder.DefineCustomItem(NpcItem.CreateItem(Lemm_Sign, "RELICDEALER_DOOR", "I didn't steal this from the door while Lemm was away... just to be clear.", "Lore", "Relic Dealer"));
        Finder.DefineCustomItem(new BoolItem()
        {
            name = Lemm_Order,
            fieldName = "lemm_Allow",
            setValue = true,
            UIDef = new MsgUIDef()
            {
                name = new BoxedString("Lemm's Order"),
                shopDesc = new BoxedString("The order of the relic seeker for a few treasure charts from Cornifer. But it seems like Lemm hasn't pay the price yet..."),
                sprite = new CustomSprite("Lemms_Order", false)
            }
        });

        // Iseldas charts
        Finder.DefineCustomLocation(new ShopLocation()
        {
            name = Iselda_Treasure,
            sceneName = "Room_mapper",
            requiredPlayerDataBool = "lemm_allow",
            fsmName = "Conversation Control",
            objectName = "Iselda",
            flingType = FlingType.DirectDeposit
        });
        List<int> chartPrices = new() { 1, 30, 69, 120, 160, 200, 230, 290, 420, 500, 750, 870, 1000, 1150 };
        for (int i = 1; i < 15; i++)
        {
            int rolledPrice = chartPrices[LoreMaster.Instance.Generator.Next(0, chartPrices.Count)];
            chartPrices.Remove(rolledPrice);
            Finder.DefineCustomItem(new BoolItem()
            {
                fieldName = Treasure_Chart_Prefix + i,
                name = Treasure_Chart_Prefix + i,
                setValue = true,
                UIDef = new MsgUIDef()
                {
                    name = new BoxedString("Treasure Chart " + i),
                    shopDesc = new BoxedString("This will lead you to a buried treasure. If it's worth the price? Well, guess you have to find that out yourself."),
                    sprite = new CustomSprite("Treasure_Chart", false)
                },
                tags = new()
                {
                    new PDBoolShopReqTag()
                    {
                        reqVal = true,
                        fieldName = "lemm_allow"
                    },
                    new CostTag() { Cost = new GeoCost(rolledPrice) }
                }
            });
        }

        // Add Treasure locations
        for (int i = 0; i < 14; i++)
            Finder.DefineCustomLocation(TreasureLocation.GenerateLocation(i));

        // Add special treasure items.
        Finder.DefineCustomItem(new BoolItem()
        {
            fieldName = "magicKey",
            name = Magical_Key,
            setValue = true,
            UIDef = new MsgUIDef()
            {
                name = new BoxedString("Magical Key"),
                shopDesc = new BoxedString("The master key of this kingdom. Opens ALMOST every locked mechanism."),
                sprite = new CustomSprite("MagicKey", false)
            }
        });
        Finder.DefineCustomItem(new BoolItem()
        {
            fieldName = "dreamMedallion",
            name = Dream_Medallion,
            setValue = true,
            UIDef = new MsgUIDef()
            {
                name = new BoxedString("Dream Medallion"),
                shopDesc = new BoxedString("An old artifact from the moth tribe. They say, the wielder of this medallion attracts the essence of dreams."),
                sprite = new CustomSprite("Dream_Medallion", false)
            }
        });
        Finder.DefineCustomItem(new BoolItem()
        {
            fieldName = "silksongJournal",
            name = Silksong_Journal,
            setValue = true,
            UIDef = new MsgUIDef()
            {
                name = new BoxedString("Silksong Journal?"),
                shopDesc = new BoxedString("A very special journal which was buried in this kingdom. The only thing on this, that Lemm could decifer, was the text \"Silksong Release Date\"."),
                sprite = new CustomSprite("Silksong_Journal", false)
            }
        });
        Finder.DefineCustomItem(new BoolItem()
        {
            fieldName = "silverSeal",
            name = Silver_Hallownest_Seal,
            setValue = true,
            UIDef = new MsgUIDef()
            {
                name = new BoxedString("Silver Seal"),
                shopDesc = new BoxedString("A very special Hallownest seal which was buried in this kingdom. I personally like the colored design more."),
                sprite = new CustomSprite("Silver_Seal", false)
            }
        });
        Finder.DefineCustomItem(new BoolItem()
        {
            fieldName = "bronzeKingIdol",
            name = Bronze_King_Idol,
            setValue = true,
            UIDef = new MsgUIDef()
            {
                name = new BoxedString("Bronze King's Idol"),
                shopDesc = new BoxedString("A very special King's Idol which was buried in this kingdom. If this color is caused by nature or artifical is beyond me."),
                sprite = new CustomSprite("Bronze_King_Idol", false)
            }
        });
        Finder.DefineCustomItem(new BoolItem()
        {
            fieldName = "goldenArcaneEgg",
            name = Golden_Arcane_Egg,
            setValue = true,
            UIDef = new MsgUIDef()
            {
                name = new BoxedString("Golden Arcane Egg"),
                shopDesc = new BoxedString("A very special arcane egg which was buried in this kingdom. Since it is one of a kind, I'm curious what kind of information it stored."),
                sprite = new CustomSprite("Golden_Egg", false)
            }
        });
    }

    private static List<AbstractPlacement> CreateTreasure()
    {
        List<AbstractPlacement> result = new();

        // Lemms door
        AbstractPlacement currentPlacement = Finder.GetLocation(Lemm_Door).Wrap();
        currentPlacement.Add(Finder.GetItem(Lemm_Sign));
        currentPlacement.Add(Finder.GetItem(Lemm_Order));
        result.Add(currentPlacement);

        // Iseldas charts
        currentPlacement = Finder.GetLocation(Iselda_Treasure).Wrap();
        for (int i = 1; i < 15; i++)
            currentPlacement.Add(Finder.GetItem($"{Treasure_Chart_Prefix}{i}"));
        result.Add(currentPlacement);

        // Place treasures.
        List<AbstractItem> treasureItems = new()
        {
            Finder.GetItem(ItemNames.Rancid_Egg),
            Finder.GetItem(ItemNames.Wanderers_Journal),
            Finder.GetItem(ItemNames.Wanderers_Journal),
            Finder.GetItem(ItemNames.Wanderers_Journal),
            Finder.GetItem(ItemNames.Hallownest_Seal),
            Finder.GetItem(ItemNames.Hallownest_Seal),
            Finder.GetItem(ItemNames.Kings_Idol),
            Finder.GetItem(ItemNames.Kings_Idol),
            Finder.GetItem(ItemNames.Arcane_Egg),
            Finder.GetItem(Silksong_Journal),
            Finder.GetItem(Silver_Hallownest_Seal),
            Finder.GetItem(Bronze_King_Idol),
            Finder.GetItem(Golden_Arcane_Egg),
            Finder.GetItem(Magical_Key),
            Finder.GetItem(Dream_Medallion)
        };

        for (int i = 1; i < 15; i++)
        {
            currentPlacement = Finder.GetLocation($"{Treasure_Prefix}{i}").Wrap();
            int rolledIndex = LoreMaster.Instance.Generator.Next(0, treasureItems.Count);
            currentPlacement.Add(treasureItems[rolledIndex]);
            treasureItems.RemoveAt(rolledIndex);
            result.Add(currentPlacement);
        }
        return result;
    }

    private static void DefineStagEgg()
    {
        Finder.DefineCustomLocation(new StagEggLocation()
        {
            name = Stag_Nest,
            sceneName = "Cliffs_03",
            flingType = FlingType.DirectDeposit
        });

        Finder.DefineCustomItem(NpcItem.CreateItem(Stag_Egg_Inspect, "STAG_EGG_INSPECT", "See this picture of an egg I took.", "Stag_Egg", "Stag"));
        Finder.DefineCustomItem(new BoolItem()
        {
            fieldName = "hasStagEgg",
            name = Stag_Egg,
            setValue = true,
            UIDef = new MsgUIDef()
            {
                name = new BoxedString("Stag Egg"),
                shopDesc = new BoxedString("If you are seeing this, something went wrong."),
                sprite = new CustomSprite("Stag_Egg", false)
            }
        });
    }

    private static AbstractPlacement CreateStagEgg()
    {
        AbstractPlacement currentPlacement = Finder.GetLocation(Stag_Nest).Wrap();
        currentPlacement.Add(Finder.GetItem(Stag_Egg_Inspect));
        currentPlacement.Add(Finder.GetItem(Stag_Egg));
        return currentPlacement;
    }

    private static void DefineExtraLore()
    {
        // Path of Pain
        Finder.DefineCustomLocation(new PathOfPainLocation()
        {
            flingType = FlingType.DirectDeposit,
            name = Path_of_Pain_End_Scene,
            sceneName = "White_Palace_20",
        });
        Finder.DefineCustomItem(new BoolItem()
        {
            fieldName = "PopLore",
            name = Path_of_Pain_Reward,
            setValue = true,
            UIDef = new MsgUIDef()
            {
                name = new BoxedString("Sacred Shell"),
                shopDesc = new BoxedString("This is a secret, that the Pale King tried to hide as best as he can."),
                sprite = new CustomSprite("Tablets/WhitePalace", false)
            }
        });

        // Record Bela
        Finder.DefineCustomLocation(new RecordBelaLocation()
        {
            sceneName = "Ruins1_30",
            flingType = FlingType.DirectDeposit,
            name = Record_Bela,
        });
        Finder.DefineCustomItem(NpcItem.CreateItem(Sanctum_Spell_Twister, "MAGE_COMP_02",
            "A hidden lore tablet deep within the Soul Sanctum.", "Tablets/CityOfTears", "Lore Tablets", false));

        Finder.DefineCustomLocation(new ZoteLocation()
        {
            sceneName = "Deepnest_33",
            flingType = FlingType.DirectDeposit,
            name = Zote_Deepnest
        });
    }

    private static List<AbstractPlacement> CreateExtraLore()
    {
        List<AbstractPlacement> extraLore = new();
        AbstractPlacement currentPlacement = Finder.GetLocation(Path_of_Pain_End_Scene).Wrap();
        currentPlacement.Add(Finder.GetItem(Path_of_Pain_Reward));
        extraLore.Add(currentPlacement);

        currentPlacement = Finder.GetLocation(Record_Bela).Wrap();
        currentPlacement.Add(Finder.GetItem(Sanctum_Spell_Twister));
        extraLore.Add(currentPlacement);

        extraLore.Add(Finder.GetLocation(Zote_Deepnest).Wrap());

        return extraLore;
    }

    private static void DefineElderbug()
    {
        Finder.DefineCustomItem(new BoolItem()
        {
            fieldName = "Read",
            name = Read_Ability,
            setValue = true,
            UIDef = new BigUIDef()
            {
                name = new BoxedString("Reading"),
                shopDesc = new BoxedString("This will be very helpful. Trust me on this one."),
                sprite = (Finder.GetItem("World_Sense").UIDef.Clone() as BigUIDef).sprite,
                descOne = new BoxedString("You finally learnt how to read!"),
                descTwo = new BoxedString("You can now comprehend the knowledge written down in the kingdom."),
                take = new BoxedString("You learnt:"),
                bigSprite = (Finder.GetItem("World_Sense").UIDef.Clone() as BigUIDef).bigSprite
            }
        });
        Finder.DefineCustomItem(new BoolItem()
        {
            name = Listen_Ability,
            fieldName = "Listen",
            setValue = true,
            UIDef = new BigUIDef()
            {
                name = new BoxedString("Listening"),
                shopDesc = new BoxedString("You should not be able to see this... I mean, you can't understand me."),
                sprite = (Finder.GetItem("World_Sense").UIDef.Clone() as BigUIDef).sprite,
                take = new BoxedString("You learnt:"),
                descOne = new BoxedString("You finally learnt how to listen!"),
                descTwo = new BoxedString("You can now \"communicate\" with the residents of Hallownest."),
                bigSprite = (Finder.GetItem("World_Sense").UIDef.Clone() as BigUIDef).bigSprite
            }
        });
        Finder.DefineCustomItem(new BoolItem()
        {
            name = Lore_Page,
            fieldName = "LorePage",
            setValue = true,
            UIDef = new BigUIDef()
            {
                name = new BoxedString("Lore Page"),
                shopDesc = new BoxedString("This will be very helpful. Trust me on this one."),
                sprite = (Finder.GetItem("Hunter's_Journal").UIDef.Clone() as BigUIDef).sprite,
                descOne = new BoxedString("You can now sense which knowledge you acquired."),
                descTwo = new BoxedString("Open the menu and navigate to the Lore Powers page to see what you've acquired so far."),
                take = new BoxedString("You got:"),
                bigSprite = (Finder.GetItem("Hunter's_Journal").UIDef.Clone() as BigUIDef).bigSprite
            }
        });
        Finder.DefineCustomItem(new BoolItem()
        {
            name = Lore_Page_Control,
            fieldName = "LorePageControl",
            setValue = true,
            UIDef = new BigUIDef()
            {
                name = new BoxedString("Lore Control"),
                shopDesc = new BoxedString("This will be very helpful. Trust me on this one."),
                sprite = (Finder.GetItem("Hunter's_Journal").UIDef.Clone() as BigUIDef).sprite,
                descOne = new BoxedString("Inserting the stone into the tablet awakens its hidden power"),
                descTwo = new BoxedString("You can now toggle unwanted powers on and off. Can only toggle obtained non cursed abilities. Only useable on a bench."),
                take = new BoxedString("Lore Tablet upgraded with:"),
                bigSprite = (Finder.GetItem("Hunter's_Journal").UIDef.Clone() as BigUIDef).bigSprite
            }
        });
        Finder.DefineCustomItem(new CleansingScrollItem()
        {
            amount = 1,
            fieldName = "CleansingScroll",
            name = Cleansing_Scroll,
            UIDef = new MsgUIDef()
            {
                name = new BoxedString("Cleansing Scroll"),
                sprite = new CustomSprite("CurseDispell", false),
                shopDesc = new BoxedString("This scroll was crafted by Elderbug to cleanse yourself from cursed lore.")
            },
            tags = new()
                    {
                        new CostTag()
                        {
                            Cost = new GeoCost(700)
                        }
                    }
        });
        Finder.DefineCustomItem(new IntItem()
        {
            amount = 1,
            fieldName = "JokerScroll",
            name = Joker_Scroll,
            UIDef = new MsgUIDef()
            {
                name = new BoxedString("Knowledge Scroll"),
                sprite = new CustomSprite("SummoningScroll", false),
                shopDesc = new BoxedString("This scroll was crafted by Elderbug to learn from his experience.")
            },
            tags = new()
            {
                new CostTag()
                {
                    Cost = new GeoCost(350)
                }
            }
        });
        Finder.DefineCustomItem(new CleansingScrollItem()
        {
            amount = 2,
            fieldName = "CleansingScroll",
            name = Cleansing_Scroll_Double,
            UIDef = new MsgUIDef()
            {
                name = new BoxedString("Cleansing Scroll Pack"),
                sprite = new CustomSprite("CurseDispell", false),
                shopDesc = new BoxedString("These scrolls were crafted by Elderbug to cleanse yourself from cursed lore.")
            },
            tags = new()
                    {
                        new CostTag()
                        {
                            Cost = new GeoCost(1400)
                        }
                    }
        });

        for (int i = 0; i < 9; i++)
            Finder.DefineCustomLocation(new ElderbugLocation()
            {
                sceneName = "Town",
                name = $"{Elderbug_Reward_Prefix}{i + 1}",
                flingType = FlingType.Everywhere
            });
    }

    private static List<AbstractPlacement> CreateElderbugRewards()
    {
        List<AbstractPlacement> placements = new();

        AbstractPlacement currentPlacement = Finder.GetLocation($"{Elderbug_Reward_Prefix}1").Wrap();
        currentPlacement.Add(Finder.GetItem(Read_Ability));
        placements.Add(currentPlacement);

        currentPlacement = Finder.GetLocation($"{Elderbug_Reward_Prefix}2").Wrap();
        currentPlacement.Add(Finder.GetItem(Listen_Ability));
        placements.Add(currentPlacement);

        currentPlacement = Finder.GetLocation($"{Elderbug_Reward_Prefix}3").Wrap();
        currentPlacement.Add(Finder.GetItem(Lore_Page));
        placements.Add(currentPlacement);

        currentPlacement = Finder.GetLocation($"{Elderbug_Reward_Prefix}4").Wrap();
        currentPlacement.Add(Finder.GetItem(Joker_Scroll));
        placements.Add(currentPlacement);

        currentPlacement = Finder.GetLocation($"{Elderbug_Reward_Prefix}5").Wrap();
        currentPlacement.Add(Finder.GetItem(Lore_Page_Control));
        placements.Add(currentPlacement);

        currentPlacement = Finder.GetLocation($"{Elderbug_Reward_Prefix}6").Wrap();
        currentPlacement.Add(Finder.GetItem(Joker_Scroll));
        placements.Add(currentPlacement);

        currentPlacement = Finder.GetLocation($"{Elderbug_Reward_Prefix}7").Wrap();
        currentPlacement.Add(Finder.GetItem(Cleansing_Scroll));
        placements.Add(currentPlacement);

        currentPlacement = Finder.GetLocation($"{Elderbug_Reward_Prefix}8").Wrap();
        currentPlacement.Add(Finder.GetItem(Joker_Scroll));
        placements.Add(currentPlacement);

        currentPlacement = Finder.GetLocation($"{Elderbug_Reward_Prefix}9").Wrap();
        currentPlacement.Add(Finder.GetItem(Cleansing_Scroll_Double));
        placements.Add(currentPlacement);

        return placements;
    } 

    #endregion
}
