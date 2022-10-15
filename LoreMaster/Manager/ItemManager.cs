using ItemChanger;
using ItemChanger.Items;
using ItemChanger.Locations;
using ItemChanger.Placements;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using LoreMaster.Enums;
using LoreMaster.Helper;
using LoreMaster.ItemChangerData;
using LoreMaster.ItemChangerData.Locations;
using LoreMaster.ItemChangerData.Locations.SpecialLocations;
using LoreMaster.ItemChangerData.Other;
using LoreMaster.ItemChangerData.UIDefs;
using LoreMaster.Randomizer;
using Modding;
using System;
using System.Collections.Generic;
using UnityEngine;
using static LoreMaster.ItemChangerData.Other.ItemList;
using static LoreMaster.ItemChangerData.Other.LocationList;

namespace LoreMaster.Manager;

/// <summary>
/// Manages all IC related stuff. (Use <seealso cref="ItemList"/> and <seealso cref="LocationList"/> if you want to use the generated stuff.)
/// </summary>
public static class ItemManager
{
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
            DefineExtraLore();
            DefineElderbug();
            if (ModHooks.GetMod("Randomizer 4") is Mod)
            {
                DefineNPC();
                DefineDreamNailReactions();
                DefinePointOfInterest();
                DefineDreamWarrior();
                DefineTraveller();
            }
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
            placements.AddRange(CreateExtraLore());
            placements.AddRange(CreateElderbugRewards());
            if ((RandomizerManager.PlayingRandomizer && !RandomizerManager.Settings.DefineRefs
                && !RandomizerManager.Settings.RandomizeTreasures) || !RandomizerManager.PlayingRandomizer)
                placements.AddRange(CreateTreasure());
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Couldn't create placements: " + exception.Message);
            LoreMaster.Instance.LogError(exception.StackTrace);
        }
        ItemChangerMod.AddPlacements(placements);
    }

    internal static T GetPlacementByName<T>(string name) where T : AbstractPlacement => ItemChanger.Internal.Ref.Settings.Placements[name] as T;

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

    private static void DefineTreasure()
    {
        // Lemms door
        Finder.DefineCustomLocation(new LemmSignLocation()
        {
            sceneName = "Ruins1_05b",
            name = Lemm_Door,
            flingType = FlingType.DirectDeposit
        });
        Finder.DefineCustomItem(Creator.CreatePowerLoreItem(Lemm_Sign, "RELICDEALER_DOOR", "Relic Dealer", TextType.Lore, new LoreUIDef()
        {
            name = new BoxedString("Lemms Sign"),
            sprite = new CustomSprite("Tablets/CityOfTears", false),
            shopDesc = new BoxedString("I didn't took this from the door... it fell to the ground and I decided I'd return it to him... later."),
            textType = TextType.Lore,
            lore = new LanguageString("Relic Dealer", "RELICDEALER_DOOR")
        }));
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

        Finder.DefineCustomLocation(new ShopLocation()
        {
            name = Iselda_Treasure,
            defaultShopItems = DefaultShopItems.IseldaCharms | DefaultShopItems.IseldaMaps
            | DefaultShopItems.IseldaMapPins | DefaultShopItems.IseldaMapMarkers | DefaultShopItems.IseldaQuill,
            requiredPlayerDataBool = "lemm_Allow",
            sceneName = "Room_mapper",
            flingType = FlingType.DirectDeposit,
            objectName = "Iselda",
            fsmName = "Conversation Control"
        });
        // Iseldas charts
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
        Finder.DefineCustomLocation(Creator.CreateInspectLocation(LocationList.Lore_Tablet_Record_Bela, "Ruins1_30", new Vector3(70f, 4.31f, .006f)));
        Finder.DefineCustomItem(Creator.CreatePowerLoreItem(ItemList.Lore_Tablet_Record_Bela, "MAGE_COMP_02", "Lore Tablets",
            TextType.Lore, new LoreUIDef()
            {
                name = new BoxedString("Overwhelming Power"),
                shopDesc = new BoxedString("A special lore tablet, which was hidden deep in Soul Sanctum."),
                textType = TextType.Lore,
                sprite = new CustomSprite("Tablets/CityOfTears", false),
                lore = new LanguageString("Lore Tablets", "MAGE_COMP_02")
            }));

        // Stag egg
        Finder.DefineCustomLocation(new StagEggLocation()
        {
            name = Stag_Nest,
            sceneName = "Cliffs_03",
            flingType = FlingType.DirectDeposit
        });
        Finder.DefineCustomItem(Creator.CreatePowerLoreItem(Stag_Egg_Inspect, "STAG_EGG_INSPECT", "Stag", TextType.Lore, new LoreUIDef()
        {
            name = new BoxedString("Stag Adoption"),
            shopDesc = new BoxedString("My thought on a stag egg I found."),
            textType = TextType.Lore,
            sprite = new CustomSprite("Tablets/Cliffs", false),
            lore = new LanguageString("Stag", "STAG_EGG_INSPECT")
        }));
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

    private static void DefineNPC()
    {
        Finder.DefineCustomLocation(Creator.CreateDialogueLocation(Bretta_Diary, "Room_Bretta", "Diary"));
        Finder.DefineCustomLocation(Creator.CreateDialogueLocation(Bardoon, "Deepnest_East_04", "Big Caterpillar"));
        Finder.DefineCustomLocation(Creator.CreateDialogueLocation(Vespa, "Hive_05", "Vespa NPC"));
        Finder.DefineCustomLocation(Creator.CreateDialogueLocation(Mask_Maker, "Room_Mask_Maker", "Maskmaker NPC"));
        Finder.DefineCustomLocation(Creator.CreateDialogueLocation(Midwife, "Deepnest_41", "NPC"));
        Finder.DefineCustomLocation(Creator.CreateDialogueLocation(Gravedigger, "Town", "Gravedigger NPC"));
        Finder.DefineCustomLocation(Creator.CreateDialogueLocation(Poggy, "Ruins_Elevator", "Ghost NPC"));
        Finder.DefineCustomLocation(Creator.CreateDialogueLocation(Joni, "Cliffs_05", "Ghost NPC Joni"));
        Finder.DefineCustomLocation(Creator.CreateDialogueLocation(Myla, "Crossroads_45", "Miner"));
        Finder.DefineCustomLocation(Creator.CreateDialogueLocation(Emilitia, "Ruins_House_03", "Emilitia NPC"));
        Finder.DefineCustomLocation(Creator.CreateDialogueLocation(Willoh, "Fungus2_34", "Giraffe NPC"));
        Finder.DefineCustomLocation(Creator.CreateDialogueLocation(Moss_Prophet, "Fungus3_39", "Moss Cultist"));
        Finder.DefineCustomLocation(Creator.CreateDialogueLocation(Fluke_Hermit, "Room_GG_Shortcut", "Fluke Hermit"));
        Finder.DefineCustomLocation(Creator.CreateDialogueLocation(Queen, "Room_Queen", "Queen"));
        Finder.DefineCustomLocation(Creator.CreateDialogueLocation(Marissa, "Ruins_Bathhouse", "Ghost NPC"));
        Finder.DefineCustomLocation(Creator.CreateDialogueLocation(Grasshopper, "Fungus1_24", "Ghost NPC"));
        Finder.DefineCustomLocation(Creator.CreateDialogueLocation(Dung_Defender, "Waterways_05", "Dung Defender NPC"));
        Finder.DefineCustomLocation(Creator.CreateDialogueLocation(Menderbug_Diary, "Room_Mender_House", "Diary"));

        Finder.DefineCustomItem(Creator.CreateNpcItem(Dialogue_Bretta_Diary, "BRETTA_DIARY_1"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Dialogue_Bardoon, "BIGCAT_TALK_01"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Dialogue_Vespa, "HIVEQUEEN_TALK", "Ghosts"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Dialogue_Mask_Maker, "MASK_MAKER_GREET"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Dialogue_Midwife, "SPIDER_MEET"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Dialogue_Gravedigger, "GRAVEDIGGER_TALK", "Ghosts"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Dialogue_Poggy, "POGGY_TALK", "Ghosts"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Dialogue_Joni, "JONI_TALK", "Ghosts"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Dialogue_Myla, "MINER_MEET_1_B"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Dialogue_Emilitia, "EMILITIA_MEET"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Dialogue_Willoh, "GIRAFFE_MEET"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Dialogue_Moss_Prophet, "MOSS_CULTIST_01"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Dialogue_Fluke_Hermit, "FLUKE_HERMIT_IDLE_1", "CP3"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Dialogue_Queen, "QUEEN_MEET"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Dialogue_Marissa, "MARISSA_TALK", "Ghosts"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Dialogue_Grasshopper, "GRASSHOPPER_TALK", "Ghosts"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Dialogue_Dung_Defender, "DUNG_DEFENDER_1"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Dialogue_Menderbug_Diary, "MENDER_DIARY", "Prompts"));
    }

    private static void DefineDreamWarrior()
    {
        Finder.DefineCustomLocation(Creator.CreateInspectLocation(Elder_Hu_Grave, "Fungus2_32", "Inspect Region Ghost"));
        Finder.DefineCustomLocation(Creator.CreateInspectLocation(Xero_Grave, "RestingGrounds_02", "Inspect Region Ghost"));
        Finder.DefineCustomLocation(Creator.CreateInspectLocation(Gorb_Grave, "Cliffs_02", "Inspect Region Ghost"));
        Finder.DefineCustomLocation(Creator.CreateInspectLocation(Marmu_Grave, "Fungus3_40", "Inspect Region Ghost"));
        Finder.DefineCustomLocation(Creator.CreateInspectLocation(No_Eyes_Statue, "Fungus1_35", "Inspect Region Ghost"));
        Finder.DefineCustomLocation(Creator.CreateInspectLocation(Markoth_Corpse, "Deepnest_East_10", "Inspect Region Ghost"));
        Finder.DefineCustomLocation(Creator.CreateInspectLocation(Galien_Corpse, "Deepnest_40", "Inspect Region Ghost"));

        Finder.DefineCustomItem(Creator.CreateNpcItem(Inspect_Elder_Hu, "HU_INSPECT", "Ghosts"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Inspect_Xero, "XERO_INSPECT", "Ghosts"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Inspect_Galien, "GALIEN_INSPECT", "Ghosts"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Inspect_Marmu, "MUMCAT_INSPECT", "Ghosts"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Inspect_Gorb, "ALADAR_INSPECT", "Ghosts"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Inspect_Markoth, "MARKOTH_INSPECT", "Ghosts"));
        Finder.DefineCustomItem(Creator.CreateNpcItem(Inspect_No_Eyes, "NOEYES_INSPECT", "Ghosts"));
    }

    private static void DefineDreamNailReactions()
    {
        Finder.DefineCustomLocation(Creator.CreateDreamImpactLocation(Aspid_Queen_Dream, "Crossroads_22"));
        Finder.DefineCustomLocation(Creator.CreateDreamImpactLocation(Mine_Golem_Dream, "Mines_31"));
        Finder.DefineCustomLocation(Creator.CreateDreamImpactLocation(Hopper_Dummy_Dream, "Deepnest_East_16"));
        Finder.DefineCustomLocation(Creator.CreateDreamImpactLocation(Ancient_Nailsmith_Golem_Dream, "Deepnest_East_14b"));
        Finder.DefineCustomLocation(Creator.CreateDreamImpactLocation(Shriek_Statue_Dream, "Abyss_12"));
        Finder.DefineCustomLocation(Creator.CreateDreamImpactLocation(Overgrown_Shaman_Dream, "Room_Fungus_Shaman"));
        Finder.DefineCustomLocation(Creator.CreateDreamImpactLocation(Shroom_King_Dream, "Fungus2_30"));
        Finder.DefineCustomLocation(Creator.CreateDreamImpactLocation(Dryya_Dream, "Fungus3_48"));
        Finder.DefineCustomLocation(Creator.CreateDreamImpactLocation(Isma_Dream, "Waterways_13"));
        Finder.DefineCustomLocation(Creator.CreateDreamImpactLocation(Radiance_Statue_Dream, "Mines_34"));
        Finder.DefineCustomLocation(Creator.CreateDreamImpactLocation(Dashmaster_Statue_Dream, "Fungus2_23"));
        Finder.DefineCustomLocation(Creator.CreateDreamImpactLocation(Snail_Shaman_Tomb_Dream, "RestingGrounds_10", "/Dream Dialogue (1)"));
        Finder.DefineCustomLocation(Creator.CreateDreamImpactLocation(Kings_Mould_Machine_Dream, "White_Palace_08"));
        Finder.DefineCustomLocation(Creator.CreateDreamImpactLocation(Dream_Shield_Statue_Dream, "RestingGrounds_17"));
        // Special dream locations.
        Finder.DefineCustomLocation(new ShadeGolemDreamLocation()
        {
            name = Shade_Golem_Dream_Normal,
            flingType = FlingType.DirectDeposit,
            GameObjectName = "Dream Dialogue_01_pre_shade_charm",
            sceneName = "Abyss_10",
            tags = new()
            {
                CreateInteropTag("Abyss_10")
            }
        });
        Finder.DefineCustomLocation(new ShadeGolemDreamLocation()
        {
            name = Shade_Golem_Dream_Void,
            flingType = FlingType.DirectDeposit,
            GameObjectName = "Dream Dialogue_02_after_shade_charm",
            sceneName = "Abyss_10",
            tags = new()
            {
                CreateInteropTag("Abyss_10")
            }
        });
        Finder.DefineCustomLocation(new PaleKingDreamLocation()
        {
            name = Pale_King_Dream,
            flingType = FlingType.DirectDeposit,
            GameObjectName = "/White King Corpse/Dream Dialogue",
            sceneName = "White_Palace_09",
            tags = new()
            {
                CreateInteropTag("White_Palace_09")
            }
        });
        Finder.DefineCustomLocation(new CrystalShamanLocation()
        {
            name = Crystalized_Shaman_Dream,
            flingType = FlingType.DirectDeposit,
            GameObjectName = "/Crystal Shaman/Dream Dialogue",
            sceneName = "Mines_35",
            tags = new()
            {
                CreateInteropTag("Mines_35")
            }
        });
        Finder.DefineCustomLocation(new GrimmSummonerDreamLocation()
        {
            name = Grimm_Summoner_Dream,
            flingType = FlingType.DirectDeposit,
            GameObjectName = "/Dream Dialogue Grimm Defeated",
            sceneName = "Cliffs_06",
            tags = new()
            {
                CreateInteropTag("Cliffs_06")
            }
        });

        string dreamSound = "Dream_Enter";
        Finder.DefineCustomItem(Creator.CreateSoundItem(Dream_Dialogue_Aspid_Queen, dreamSound,
            Creator.CreateDreamUIDef(Dream_Dialogue_Aspid_Queen, "HATCHLING_QUEEN_CORPSE", "Lore Tablets", new("Aspid_Queen"))));

        Finder.DefineCustomItem(Creator.CreateSoundItem(Dream_Dialogue_Mine_Golem, dreamSound,
            Creator.CreateDreamUIDef(Dream_Dialogue_Mine_Golem, "DREAM_MINES_ROBOT", "Lore Tablets", new("Mine_Golem"))));

        Finder.DefineCustomItem(Creator.CreateSoundItem(Dream_Dialogue_Hopper_Dummy, dreamSound,
            Creator.CreateDreamUIDef(Dream_Dialogue_Hopper_Dummy, "DREAM_DUMMY", "Lore Tablets", new("Great_Hopper_Dummy"))));

        Finder.DefineCustomItem(Creator.CreateSoundItem(Dream_Dialogue_Ancient_Nailsmith_Golem, dreamSound,
            Creator.CreateDreamUIDef(Dream_Dialogue_Ancient_Nailsmith_Golem, "NAILSMITH_ANCIENT", "Lore Tablets", new("Ancient_Nailsmith"))));

        Finder.DefineCustomItem(Creator.CreateSoundItem(Dream_Dialogue_Shriek_Statue, dreamSound,
            Creator.CreateDreamUIDef(Dream_Dialogue_Shriek_Statue, "SCREAM_ALTAR", "Lore Tablets", new("Shriek_Statue"))));

        Finder.DefineCustomItem(Creator.CreateSoundItem(Dream_Dialogue_Shade_Golem_Normal, dreamSound,
            Creator.CreateDreamUIDef(Dream_Dialogue_Shade_Golem_Normal, "SHADE_BEAST_01", "Lore Tablets", new("Shade_Golem"))));

        Finder.DefineCustomItem(Creator.CreateSoundItem(Dream_Dialogue_Shade_Golem_Void, dreamSound,
            Creator.CreateDreamUIDef(Dream_Dialogue_Shade_Golem_Void, "SHADE_BEAST_02", "Lore Tablets", new("Shade_Golem"))));

        Finder.DefineCustomItem(Creator.CreateSoundItem(Dream_Dialogue_Overgrown_Shaman, dreamSound,
            Creator.CreateDreamUIDef(Dream_Dialogue_Overgrown_Shaman, "SHAMAN_FUNG_DREAM", "Shaman", new("Shaman_Corpse"))));

        Finder.DefineCustomItem(Creator.CreateSoundItem(Dream_Dialogue_Crystalized_Shaman, dreamSound,
            Creator.CreateDreamUIDef(Dream_Dialogue_Crystalized_Shaman, "SHAMAN_CRYSTAL_DREAM", "Shaman", new("Crystal_Shaman"))));

        Finder.DefineCustomItem(Creator.CreateSoundItem(Dream_Dialogue_Shroom_King, dreamSound,
            Creator.CreateDreamUIDef(Dream_Dialogue_Shroom_King, "FUNG_SHROOM_DREAM", "Lore Tablets", new("Shroom_King"))));

        Finder.DefineCustomItem(Creator.CreateSoundItem(Dream_Dialogue_Dryya, dreamSound,
            Creator.CreateDreamUIDef(Dream_Dialogue_Dryya, "WHITE_KNIGHT_CORPSE", "Lore Tablets", new("Dryya"))));

        Finder.DefineCustomItem(Creator.CreateSoundItem(Dream_Dialogue_Isma, dreamSound,
            Creator.CreateDreamUIDef(Dream_Dialogue_Isma, "ISMA_DREAM", "Lore Tablets", new("Isma"))));

        Finder.DefineCustomItem(Creator.CreateSoundItem(Dream_Dialogue_Radiance_Statue, dreamSound,
            Creator.CreateDreamUIDef(Dream_Dialogue_Radiance_Statue, "MOTH_STATUE", "Lore Tablets", new("Radiance_Statue"))));

        Finder.DefineCustomItem(Creator.CreateSoundItem(Dream_Dialogue_Dashmaster_Statue, dreamSound,
            Creator.CreateDreamUIDef(Dream_Dialogue_Dashmaster_Statue, "DASH_MASTER_STATUE", "Lore Tablets", new("Dashmaster"))));

        Finder.DefineCustomItem(Creator.CreateSoundItem(Dream_Dialogue_Snail_Shaman_Tomb, dreamSound,
            Creator.CreateDreamUIDef(Dream_Dialogue_Snail_Shaman_Tomb, "SARCOPHAGUS_01", "Lore Tablets", new("Shaman_Tomb"))));

        Finder.DefineCustomItem(Creator.CreateSoundItem(Dream_Dialogue_Pale_King, dreamSound,
            Creator.CreateDreamUIDef(Dream_Dialogue_Pale_King, "KING_FINAL_WORDS", "Lore Tablets", new("Pale_King"))));

        Finder.DefineCustomItem(Creator.CreateSoundItem(Dream_Dialogue_Grimm_Summoner, dreamSound,
            Creator.CreateDreamUIDef(Dream_Dialogue_Grimm_Summoner, "GRIMMSYCOPHANT_DREAM", "CP2", new("Grimm_Summoner"))));

        Finder.DefineCustomItem(Creator.CreateSoundItem(Dream_Dialogue_Kings_Mould_Machine, dreamSound,
            Creator.CreateDreamUIDef(Dream_Dialogue_Kings_Mould_Machine, "KING_WORKSHOP_MOULD", "Lore Tablets", new("Kings_Mould_Machine"))));
        InteropTag interopTag = new();
        Finder.DefineCustomItem(Creator.CreateSoundItem(Dream_Dialogue_Dream_Shield_Statue, dreamSound,
            Creator.CreateDreamUIDef(Dream_Dialogue_Dream_Shield_Statue, "MOTHSTONE_DREAM", "CP2", new("Dream_Shield"))));
    }

    private static void DefinePointOfInterest()
    {
        Finder.DefineCustomLocation(Creator.CreateDialogueLocation(City_Fountain, "Ruins1_27", "Fountain Inspect"));
        Finder.DefineCustomLocation(Creator.CreateDialogueLocation(Dreamer_Tablet, "RestingGrounds_04", "Dreamer Plaque Inspect"));
        Finder.DefineCustomLocation(Creator.CreateInspectLocation(Beast_Den_Altar, "Deepnest_Spider_Town", new Vector3(63.85f, 114.41f)));
        Finder.DefineCustomLocation(Creator.CreateInspectLocation(Weaver_Seal, "Deepnest_45_v02", new Vector3(12.29f, 43.41f)));
        Finder.DefineCustomLocation(Creator.CreateInspectLocation(Grimm_Machine, "Grimm_Main_Tent", new Vector3(83f, 45.41f)));
        Finder.DefineCustomLocation(Creator.CreateInspectLocation(Garden_Golem, "Fungus1_23", new Vector3(20.18f, 7.41f)));
        Finder.DefineCustomLocation(Creator.CreateInspectLocation(Grub_Seal, "Ruins2_11", new Vector3(53.91f, 129.41f)));
        Finder.DefineCustomLocation(Creator.CreateInspectLocation(White_Palace_Nursery, "White_Palace_09", new Vector3(87.22f, 31.41f)));
        Finder.DefineCustomLocation(Creator.CreateInspectLocation(Grimm_Summoner_Corpse, "Cliffs_06", "/Sycophant Dream/Inspect Region"));

        Finder.DefineCustomItem(Creator.CreatePowerLoreItem(Inscription_City_Fountain, "FOUNTAIN_PLAQUE_DESC", "Lore Tablets", TextType.Lore, new LoreUIDef()
        {
            name = new BoxedString("City Fountain"),
            shopDesc = new BoxedString(Properties.ShopDescriptions.Fountain),
            sprite = new CustomSprite("Fountain"),
            textType = TextType.Lore,
            lore = new LanguageString("Lore Tablets", "FOUNTAIN_PLAQUE_DESC")
        }));
        Finder.DefineCustomItem(Creator.CreatePowerLoreItem(Inscription_Dreamer_Tablet, "DREAMERS_INSPECT_RG5", "Lore Tablets", TextType.Lore, new BigUIDef()
        {
            name = new BoxedString("Dreamer Tablet"),
            shopDesc = new BoxedString(Properties.ShopDescriptions.Dreamer_Tablet),
            descOne = new BoxedString("To protect the Vessel, the Dreamers lay sleeping."),
            take = new BoxedString("You thought this was a dreamer, didn't you?"),
            bigSprite = (Finder.GetItem(ItemNames.Dreamer).GetResolvedUIDef() as BigUIDef).bigSprite,
            sprite = new CustomSprite("Dreamer_Plaque"),
            descTwo = new LanguageString("Lore Tablets", "DREAMERS_INSPECT_RG5")
        }));
        Finder.DefineCustomItem(Creator.CreateSoundItem(Inspect_Beast_Den_Altar, "Secret", new LoreUIDef()
        {
            name = new BoxedString("Spider Altar?"),
            shopDesc = new BoxedString(Properties.ShopDescriptions.Fountain),
            sprite = new CustomSprite("Spider_Shrine"),
            lore = new BoxedString(Properties.InspectText.Beast_Den),
            textType = TextType.Lore,
        }));
        Finder.DefineCustomItem(Creator.CreateSoundItem(Inspect_Weaver_Seal, "Secret", new LoreUIDef()
        {
            name = new BoxedString("Weaver Seal"),
            shopDesc = new BoxedString(Properties.ShopDescriptions.Fountain),
            sprite = new CustomSprite("Weaver_Seal"),
            lore = new BoxedString(Properties.InspectText.Weaver_Binding),
            textType = TextType.Lore,
        }));
        Finder.DefineCustomItem(Creator.CreateSoundItem(Inspect_Grimm_Machine, "Secret", new LoreUIDef()
        {
            name = new BoxedString("Grimm Machine"),
            shopDesc = new BoxedString(Properties.ShopDescriptions.Fountain),
            sprite = new CustomSprite("Lore", false),
            lore = new BoxedString(Properties.InspectText.Grimm_Machine),
            textType = TextType.Lore,
        }));
        Finder.DefineCustomItem(Creator.CreateSoundItem(Inspect_Garden_Golem, "Secret", new LoreUIDef()
        {
            name = new BoxedString("Garden Golem"),
            shopDesc = new BoxedString(Properties.ShopDescriptions.Fountain),
            sprite = new CustomSprite("Garden_Golem"),
            lore = new BoxedString(Properties.InspectText.Garden_Golem),
            textType = TextType.Lore,
        }));
        Finder.DefineCustomItem(Creator.CreateSoundItem(Inspect_Grub_Seal, "Secret", new LoreUIDef()
        {
            name = new BoxedString("Grub Painting"),
            shopDesc = new BoxedString(Properties.ShopDescriptions.Fountain),
            sprite = new CustomSprite("vitruvian_grub"),
            lore = new BoxedString(Properties.InspectText.Grub_Seal),
            textType = TextType.Lore,
        }));
        Finder.DefineCustomItem(Creator.CreateSoundItem(Inspect_White_Palace_Nursery, "Secret", new LoreUIDef()
        {
            name = new BoxedString("White Palace Nursery"),
            shopDesc = new BoxedString(Properties.ShopDescriptions.Fountain),
            sprite = new CustomSprite("Nursery"),
            lore = new BoxedString(Properties.InspectText.White_Palace_Nursery),
            textType = TextType.Lore,
        }));
        Finder.DefineCustomItem(Creator.CreateSoundItem(Inspect_Grimm_Summoner_Corpse, "Secret", new LoreUIDef()
        {
            name = new BoxedString("Grimm Summoner Corpse"),
            shopDesc = new BoxedString(Properties.ShopDescriptions.Fountain),
            sprite = new CustomSprite("Grimm_Summoner"),
            lore = new BoxedString(Language.Language.Get("CP2", "GRIMMSYCOPHANT_INSPECT")),
            textType = TextType.Lore,
        }));
    }

    private static void DefineTraveller()
    {
        // Quirrel
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Quirrel_Crossroads, 0, Traveller.Quirrel));
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Quirrel_Greenpath, 1, Traveller.Quirrel));
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Quirrel_Queen_Station, 2, Traveller.Quirrel));
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Quirrel_Mantis_Village, 3, Traveller.Quirrel));
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Quirrel_City, 4, Traveller.Quirrel));
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Quirrel_Peaks, 5, Traveller.Quirrel));
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Quirrel_Deepnest, 6, Traveller.Quirrel));
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Quirrel_Outside_Archive, 7, Traveller.Quirrel));
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Quirrel_After_Monomon, 8, Traveller.Quirrel));
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Quirrel_Blue_Lake, 9, Traveller.Quirrel));

        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Quirrel_Crossroads, "QUIRREL_TEMPLE_1", Traveller.Quirrel));
        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Quirrel_Greenpath, "QUIRREL_GREENPATH_1", Traveller.Quirrel));
        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Quirrel_Queen_Station, "QUIRREL_QUEENSTATION_01", Traveller.Quirrel));
        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Quirrel_Mantis_Village, "QUIRREL_MANTIS_01", Traveller.Quirrel));
        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Quirrel_City, "QUIRREL_RUINS_1", Traveller.Quirrel));
        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Quirrel_Deepnest, "QUIRREL_SPA", Traveller.Quirrel));
        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Quirrel_Peaks, "QUIRREL_MINES_1", Traveller.Quirrel));
        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Quirrel_Outside_Archive, "QUIRREL_FOGCANYON_A", Traveller.Quirrel));
        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Quirrel_Archive, "QUIRREL_TEACHER_3", Traveller.Quirrel));
        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Quirrel_Blue_Lake, "QUIRREL_EPILOGUE_A", Traveller.Quirrel));

        // Cloth
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Cloth_Fungal_Wastes, 0, Traveller.Cloth));
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Cloth_Basin, 1, Traveller.Cloth));
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Cloth_Deepnest, 2, Traveller.Cloth));
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Cloth_Garden, 3, Traveller.Cloth));
        // Cloth ghost and her town appearance is coupled, to still give the player the choice.
        Finder.DefineCustomLocation(new DualLocation()
        {
            name = "Cloth_End",
            flingType = FlingType.DirectDeposit,
            sceneName = "",
            Test = new ClothTest(),
            trueLocation = TravellerLocation.CreateTravellerLocation(Cloth_Ghost, 4, Traveller.Cloth),
            falseLocation = TravellerLocation.CreateTravellerLocation(Cloth_Town, 5, Traveller.Cloth)
        });

        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Cloth_Fungal_Wastes, "CLOTH_FUNG_MEET", Traveller.Cloth));
        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Cloth_Basin, "CLOTH_TRAMWAY_MEET", Traveller.Cloth));
        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Cloth_Deepnest, "CLOTH_DEEPNEST_MEET", Traveller.Cloth));
        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Cloth_Garden, "CLOTH_QG_GREET", Traveller.Cloth));
        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Cloth_Ghost, "CLOTH_GHOST", Traveller.Cloth));

        // Tiso
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Tiso_Dirtmouth, 0, Traveller.Tiso));
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Tiso_Crossroads, 1, Traveller.Tiso));
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Tiso_Blue_Lake, 2, Traveller.Tiso));
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Tiso_Colosseum, 3, Traveller.Tiso));
        Finder.DefineCustomLocation(Creator.CreateDreamImpactLocation(Tiso_Corpse, "Deepnest_East_07", "/tiso_corpse/Dream Dialogue"));

        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Tiso_Dirtmouth, "TISO_TOWN_GREET", Traveller.Tiso));
        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Tiso_Crossroads, "TISO_BENCH_GREET", Traveller.Tiso));
        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Tiso_Blue_Lake, "TISO_LAKE_GREET", Traveller.Tiso));
        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Tiso_Colosseum, "TISO_COLOSSEUM", Traveller.Tiso));
        Finder.DefineCustomItem(Creator.CreateSoundItem(Dream_Dialogue_Tiso_Corpse, "Dream_Enter", new DreamLoreUIDef()
        {
            name = new BoxedString("Tiso (5 / 5)"),
            Key = "TISO_CORPSE",
            Sheet = "Lore Tablets",
            shopDesc = new BoxedString("Well... his glory didn't turn out the way he expected."),
            sprite = new CustomSprite("tiso_corpse")
        }));

        // Zote
        Finder.DefineCustomLocation(new ZoteGreenpathLocation()
        {
            name = Zote_Greenpath,
            flingType = FlingType.DirectDeposit,
            FsmName = "Conversation Control",
            ObjectName = "Zote Buzzer Convo(Clone)",
            sceneName = "Fungus1_20_v02",
            tags = new()
            {
                CreateInteropTag("Fungus1_20_v02")
            }
        });
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Zote_Dirtmouth_Intro, 1, Traveller.Zote));
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Zote_City, 2, Traveller.Zote));
        Finder.DefineCustomLocation(new ZoteDeepnestLocation()
        {
            ObjectName = "/Zote Deepnest/Faller/NPC",
            FsmName = "Conversation Control",
            name = Zote_Deepnest,
            sceneName = "Deepnest_33",
            flingType = FlingType.DirectDeposit,
            tags = new()
            {
                CreateInteropTag("Deepnest_33")
            }
        });
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Zote_Colosseum, 4, Traveller.Zote));
        Finder.DefineCustomLocation(TravellerLocation.CreateTravellerLocation(Zote_Dirtmouth_After_Colosseum, 5, Traveller.Zote));

        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Zote_Greenpath, "ZOTE_BUZZER_1", Traveller.Zote));
        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Zote_Dirtmouth_Intro, "ZOTE_TOWN_1", Traveller.Zote));
        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Zote_City, "ZOTE_CITY_GREET", Traveller.Zote));
        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Zote_Deepnest, "ZOTE_DEEPNEST_1", Traveller.Zote));
        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Zote_Colosseum, "ZOTE_COLOSSEUM_MEET", Traveller.Zote));
        Finder.DefineCustomItem(Creator.CreateTravellerItem(Dialogue_Zote_Dirtmouth_After_Colosseum, "PRECEPT_1_R", Traveller.Zote));
    }

    #endregion

    #region Placing

    private static AbstractPlacement CreatePlacement(string locationName, string itemName)
    {
        AbstractPlacement placement = Finder.GetLocation(locationName).Wrap();
        placement.Items.Add(Finder.GetItem(itemName));
        return placement;
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

    private static List<AbstractPlacement> CreateTreasure()
    {
        List<AbstractPlacement> result = new();
        AbstractPlacement currentPlacement;
        // Lemms door
        if (!RandomizerManager.PlayingRandomizer || (!RandomizerManager.Settings.RandomizeTreasures && !RandomizerManager.Settings.DefineRefs))
        {
            currentPlacement = Finder.GetLocation(Lemm_Door).Wrap();
            currentPlacement.Add(Finder.GetItem(Lemm_Sign));
            currentPlacement.Add(Finder.GetItem(Lemm_Order));
            result.Add(currentPlacement);

            // Iseldas charts
            currentPlacement = Finder.GetLocation(Iselda_Treasure).Wrap();
            if (ItemChanger.Internal.Ref.Settings.Placements.ContainsKey(LocationNames.Iselda))
            {
                currentPlacement = ItemChanger.Internal.Ref.Settings.Placements[LocationNames.Iselda];
                for (int i = 1; i < 15; i++)
                    currentPlacement.Add(Finder.GetItem($"{Treasure_Chart_Prefix}{i}"));
            }
            else
            {
                for (int i = 1; i < 15; i++)
                    currentPlacement.Add(Finder.GetItem($"{Treasure_Chart_Prefix}{i}"));
                result.Add(currentPlacement);
            }
        }

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

        string[] treasureLocation = new string[]
        {
            Treasure_Ancient_Basin,
            Treasure_City_Of_Tears,
            Treasure_Crossroads,
            Treasure_Crystal_Peaks,
            Treasure_Deepnest,
            Treasure_Fog_Canyon,
            Treasure_Fungal_Wastes,
            Treasure_Greenpath,
            Treasure_Howling_Cliffs,
            Treasure_Kingdoms_Edge,
            Treasure_Queens_Garden,
            Treasure_Resting_Grounds,
            Treasure_Waterways,
            Treasure_White_Palace
        };

        for (int i = 0; i < 14; i++)
        {
            currentPlacement = Finder.GetLocation($"{treasureLocation[i]}").Wrap();
            int rolledIndex = LoreMaster.Instance.Generator.Next(0, treasureItems.Count);
            currentPlacement.Add(treasureItems[rolledIndex]);
            treasureItems.RemoveAt(rolledIndex);
            result.Add(currentPlacement);
        }
        return result;
    }

    private static List<AbstractPlacement> CreateExtraLore()
    {
        List<AbstractPlacement> extraLore = new();
        AbstractPlacement currentPlacement = Finder.GetLocation(Path_of_Pain_End_Scene).Wrap();
        currentPlacement.Add(Finder.GetItem(Path_of_Pain_Reward));
        extraLore.Add(currentPlacement);

        if (RandomizerManager.PlayingRandomizer && ItemChanger.Internal.Ref.Settings.Placements.ContainsKey(Stag_Nest) 
            && (RandomizerManager.Settings.RandomizePointsOfInterest || RandomizerManager.Settings.DefineRefs))
        {
            AbstractPlacement placement = ItemChanger.Internal.Ref.Settings.Placements[Stag_Nest];
            placement.Items.Add(Finder.GetItem(Stag_Egg));
        }
        else
        {
            currentPlacement = Finder.GetLocation(LocationList.Lore_Tablet_Record_Bela).Wrap();
            currentPlacement.Add(Finder.GetItem(ItemList.Lore_Tablet_Record_Bela));
            extraLore.Add(currentPlacement);

            currentPlacement = Finder.GetLocation(Stag_Nest).Wrap();
            currentPlacement.Add(Finder.GetItem(Stag_Egg_Inspect));
            currentPlacement.Add(Finder.GetItem(Stag_Egg));
        }

        return extraLore;
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

    internal static InteropTag CreateInteropTag(string sceneName)
    {
        return new()
        {
            Message = "RandoSupplementalMetadata",
            Properties = new()
            {
                {"ModSource", nameof(LoreMaster)},
                {"PinSprite", new CustomSprite("Lore", false)},
                {"SceneName", sceneName },
                {"MapLocations", (sceneName, 0f, 0f) }
            }
        };
    }

    #endregion
}
