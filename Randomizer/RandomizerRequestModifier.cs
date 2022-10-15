using ItemChanger;
using LoreMaster.Enums;
using RandomizerMod.RC;
using System.Collections.Generic;
using static ItemChanger.ItemNames;
using static LoreMaster.ItemChangerData.Other.ItemList;
using static LoreMaster.ItemChangerData.Other.LocationList;

namespace LoreMaster.Randomizer;

internal static class RandomizerRequestModifier
{
    #region Members

    internal static string[] NpcLocations { get; } = new string[]
    {
        Bretta_Diary,
        Bardoon,
        Vespa,
        Midwife,
        Myla,
        Willoh,
        Marissa,
        Joni,
        Grasshopper,
        Mask_Maker,
        Emilitia,
        Fluke_Hermit,
        Moss_Prophet,
        Queen,
        Dung_Defender,
        Menderbug_Diary,
        Gravedigger,
        Poggy
    };

    internal static string[] NpcItems = new string[]
    {
        Dialogue_Bretta_Diary,
        Dialogue_Bardoon,
        Dialogue_Vespa,
        Dialogue_Midwife,
        Dialogue_Myla,
        Dialogue_Willoh,
        Dialogue_Marissa,
        Dialogue_Joni,
        Dialogue_Grasshopper,
        Dialogue_Mask_Maker,
        Dialogue_Emilitia,
        Dialogue_Fluke_Hermit,
        Dialogue_Moss_Prophet,
        Dialogue_Queen,
        Dialogue_Dung_Defender,
        Dialogue_Menderbug_Diary,
        Dialogue_Gravedigger,
        Dialogue_Poggy
    };

    internal static string[] DreamWarriorLocations = new string[]
    {
        Elder_Hu_Grave,
        Gorb_Grave,
        Marmu_Grave,
        Xero_Grave,
        No_Eyes_Statue,
        Markoth_Corpse,
        Galien_Corpse
    };

    internal static string[] DreamWarriorItems = new string[]
    {
        Inspect_Elder_Hu,
        Inspect_Gorb,
        Inspect_Marmu,
        Inspect_Xero,
        Inspect_No_Eyes,
        Inspect_Markoth,
        Inspect_Galien
    };

    internal static string[] DreamLocations = new string[]
    {
        Ancient_Nailsmith_Golem_Dream,
        Aspid_Queen_Dream,
        Crystalized_Shaman_Dream,
        Dashmaster_Statue_Dream,
        Dream_Shield_Statue_Dream,
        Dryya_Dream,
        Grimm_Summoner_Dream,
        Hopper_Dummy_Dream,
        Isma_Dream,
        Kings_Mould_Machine_Dream,
        Mine_Golem_Dream,
        Overgrown_Shaman_Dream,
        Pale_King_Dream,
        Radiance_Statue_Dream,
        Shade_Golem_Dream_Normal,
        Shade_Golem_Dream_Void,
        Shriek_Statue_Dream,
        Shroom_King_Dream,
        Snail_Shaman_Tomb_Dream
    };

    internal static string[] DreamItems = new string[]
    {
        Dream_Dialogue_Ancient_Nailsmith_Golem,
        Dream_Dialogue_Aspid_Queen,
        Dream_Dialogue_Crystalized_Shaman,
        Dream_Dialogue_Dashmaster_Statue,
        Dream_Dialogue_Dream_Shield_Statue,
        Dream_Dialogue_Dryya,
        Dream_Dialogue_Grimm_Summoner,
        Dream_Dialogue_Hopper_Dummy,
        Dream_Dialogue_Isma,
        Dream_Dialogue_Kings_Mould_Machine,
        Dream_Dialogue_Mine_Golem,
        Dream_Dialogue_Overgrown_Shaman,
        Dream_Dialogue_Pale_King,
        Dream_Dialogue_Radiance_Statue,
        Dream_Dialogue_Shade_Golem_Normal,
        Dream_Dialogue_Shade_Golem_Void,
        Dream_Dialogue_Shriek_Statue,
        Dream_Dialogue_Shroom_King,
        Dream_Dialogue_Snail_Shaman_Tomb
    };

    internal static string[] PointOfInterestLocations = new string[]
    {
        City_Fountain,
        Dreamer_Tablet,
        Weaver_Seal,
        Grimm_Machine,
        Beast_Den_Altar,
        Garden_Golem,
        Grub_Seal,
        Path_Of_Pain_Seal,
        White_Palace_Nursery,
        Grimm_Summoner_Corpse,
        Stag_Nest
    };

    internal static string[] PointOfInterestItems = new string[]
    {
        Inscription_City_Fountain,
        Inscription_Dreamer_Tablet,
        Inspect_Weaver_Seal,
        Inspect_Grimm_Machine,
        Inspect_Beast_Den_Altar,
        Inspect_Garden_Golem,
        Inspect_Grub_Seal,
        Inspect_Path_Of_Pain_Seal,
        Inspect_White_Palace_Nursery,
        Inspect_Grimm_Summoner_Corpse,
        Stag_Egg_Inspect
    };

    internal static string[] TravellerLocations = new string[]
    {
        Quirrel_Crossroads,
        Quirrel_Greenpath,
        Quirrel_Queen_Station,
        Quirrel_Mantis_Village,
        Quirrel_City,
        Quirrel_Deepnest,
        Quirrel_Peaks,
        Quirrel_Outside_Archive,
        Quirrel_After_Monomon,
        Quirrel_Blue_Lake,
        Cloth_Fungal_Wastes,
        Cloth_Basin,
        Cloth_Deepnest,
        Cloth_Garden,
        Cloth_End,
        Tiso_Dirtmouth,
        Tiso_Crossroads,
        Tiso_Blue_Lake,
        Tiso_Colosseum,
        Tiso_Corpse,
        Zote_Greenpath,
        Zote_Dirtmouth_Intro,
        Zote_City,
        Zote_Deepnest,
        Zote_Colosseum,
        Zote_Dirtmouth_After_Colosseum
    };

    internal static string[] TravellerItems = new string[]
    {
        Dialogue_Quirrel_Crossroads,
        Dialogue_Quirrel_Greenpath,
        Dialogue_Quirrel_Queen_Station,
        Dialogue_Quirrel_Mantis_Village,
        Dialogue_Quirrel_City,
        Dialogue_Quirrel_Deepnest,
        Dialogue_Quirrel_Peaks,
        Dialogue_Quirrel_Outside_Archive,
        Dialogue_Quirrel_Archive,
        Dialogue_Quirrel_Blue_Lake,
        Dialogue_Cloth_Fungal_Wastes,
        Dialogue_Cloth_Basin,
        Dialogue_Cloth_Deepnest,
        Dialogue_Cloth_Garden,
        Dialogue_Cloth_Ghost,
        Dialogue_Tiso_Dirtmouth,
        Dialogue_Tiso_Crossroads,
        Dialogue_Tiso_Blue_Lake,
        Dialogue_Tiso_Colosseum,
        Dream_Dialogue_Tiso_Corpse,
        Dialogue_Zote_Greenpath,
        Dialogue_Zote_Dirtmouth_Intro,
        Dialogue_Zote_City,
        Dialogue_Zote_Deepnest,
        Dialogue_Zote_Colosseum,
        Dialogue_Zote_Dirtmouth_After_Colosseum
    };

    internal static string[] TreasureLocation = new string[]
    {
        Treasure_Howling_Cliffs,
        Treasure_Crossroads,
        Treasure_Greenpath,
        Treasure_Fog_Canyon,
        Treasure_Fungal_Wastes,
        Treasure_City_Of_Tears,
        Treasure_Waterways,
        Treasure_Deepnest,
        Treasure_Ancient_Basin,
        Treasure_Kingdoms_Edge,
        Treasure_Crystal_Peaks,
        Treasure_Resting_Grounds,
        Treasure_Queens_Garden,
        Treasure_White_Palace
    };

    internal static string[] TreasureItems = new string[]
    {
        Magical_Key,
        Dream_Medallion,
        Silksong_Journal,
        Silver_Hallownest_Seal,
        Bronze_King_Idol,
        Golden_Arcane_Egg
    };

    internal static Dictionary<Area, string[]> LoreItem = new()
    {
       { Area.AncientBasin, new string[] {Lore_Tablet_Ancient_Basin } },
       { Area.FogCanyon, new string[]{ Lore_Tablet_Archives_Left,
        Lore_Tablet_Archives_Right,
        Lore_Tablet_Archives_Upper } },
       { Area.CityOfTears, new string[]{ Lore_Tablet_City_Entrance,
        Lore_Tablet_Pleasure_House,
        Lore_Tablet_Sanctum_Entrance,
        Lore_Tablet_Sanctum_Past_Soul_Master,
        Lore_Tablet_Watchers_Spire } },
        { Area.WaterWays, new string[]{Lore_Tablet_Dung_Defender } },
        { Area.FungalWastes, new string[]{Lore_Tablet_Fungal_Core,
        Lore_Tablet_Fungal_Wastes_Below_Shrumal_Ogres,
        Lore_Tablet_Fungal_Wastes_Hidden,
        Lore_Tablet_Pilgrims_Way_2,
        Lore_Tablet_Spore_Shroom,
        Lore_Tablet_Mantis_Outskirts,
        Lore_Tablet_Mantis_Village } },
        { Area.Greenpath, new string[]{Lore_Tablet_Greenpath_Below_Toll,
        Lore_Tablet_Greenpath_Lifeblood,
        Lore_Tablet_Greenpath_Lower_Hidden,
        Lore_Tablet_Greenpath_QG,
        Lore_Tablet_Greenpath_Stag,
        Lore_Tablet_Greenpath_Upper_Hidden } },
        { Area.Cliffs, new string[]{Lore_Tablet_Howling_Cliffs } },
        { Area.KingdomsEdge, new string[]{Lore_Tablet_Kingdoms_Edge } },
        { Area.Dirtmouth, new string[]{Lore_Tablet_Kings_Pass_Exit,
        Lore_Tablet_Kings_Pass_Focus,
        Lore_Tablet_Kings_Pass_Fury } },
        { Area.WhitePalace, new string[]{Lore_Tablet_Palace_Throne,
        Lore_Tablet_Palace_Workshop,
        Lore_Tablet_Path_of_Pain_Entrance } },
        { Area.Crossroads, new string[]{Lore_Tablet_World_Sense,
        Lore_Tablet_Pilgrims_Way_1 } }
    };

    #endregion

    public static void ModifyRequest()
    {
        RequestBuilder.OnUpdate.Subscribe(30f, AddLoreMasterExtra);
    }

    private static void AddLoreMasterExtra(RequestBuilder requestBuilder)
    {
        if (RandomizerManager.Settings.RandomizeNpc)
        {
            for (int i = 0; i < NpcItems.Length; i++)
            {
                requestBuilder.AddItemByName(NpcItems[i]);
                requestBuilder.AddLocationByName(NpcLocations[i]);
            }
            EditRequests(NpcItems, NpcLocations, requestBuilder);
        }
        else if (RandomizerManager.Settings.DefineRefs)
        {
            for (int i = 0; i < NpcLocations.Length; i++)
                requestBuilder.AddToVanilla(NpcItems[i], NpcLocations[i]);
            EditRequests(NpcItems, NpcLocations, requestBuilder);
        }

        if (RandomizerManager.Settings.RandomizeDreamWarriorStatues)
        {
            for (int i = 0; i < DreamWarriorLocations.Length; i++)
            {
                requestBuilder.AddItemByName(DreamWarriorItems[i]);
                requestBuilder.AddLocationByName(DreamWarriorLocations[i]);
            }
            EditRequests(DreamWarriorItems, DreamWarriorLocations, requestBuilder);
        }
        else if (RandomizerManager.Settings.DefineRefs)
        {
            for (int i = 0; i < DreamWarriorLocations.Length; i++)
                requestBuilder.AddToVanilla(DreamWarriorItems[i], DreamWarriorLocations[i]);
            EditRequests(DreamWarriorItems, DreamWarriorLocations, requestBuilder);
        }

        if (RandomizerManager.Settings.RandomizeDreamDialogue)
        {
            for (int i = 0; i < DreamLocations.Length; i++)
            {
                requestBuilder.AddItemByName(DreamItems[i]);
                requestBuilder.AddLocationByName(DreamLocations[i]);
            }
            EditRequests(DreamItems, DreamLocations, requestBuilder);
        }
        else if (RandomizerManager.Settings.DefineRefs)
        {
            for (int i = 0; i < DreamLocations.Length; i++)
                requestBuilder.AddToVanilla(DreamItems[i], DreamLocations[i]);
            EditRequests(DreamItems, DreamLocations, requestBuilder);
        }

        if (RandomizerManager.Settings.RandomizePointsOfInterest)
        {
            for (int i = 0; i < PointOfInterestLocations.Length; i++)
            {
                requestBuilder.AddItemByName(PointOfInterestItems[i]);
                requestBuilder.AddLocationByName(PointOfInterestLocations[i]);
            }
            EditRequests(PointOfInterestItems, PointOfInterestLocations, requestBuilder);
        }
        else if (RandomizerManager.Settings.DefineRefs)
        {
            for (int i = 0; i < PointOfInterestLocations.Length; i++)
                requestBuilder.AddToVanilla(PointOfInterestItems[i], PointOfInterestLocations[i]);
            EditRequests(PointOfInterestItems, PointOfInterestLocations, requestBuilder);
        }

        if (RandomizerManager.Settings.RandomizeTravellers)
        {
            for (int i = 0; i < TravellerLocations.Length; i++)
            {
                requestBuilder.AddItemByName(TravellerItems[i]);
                requestBuilder.AddLocationByName(TravellerLocations[i]);
            }
            EditRequests(TravellerItems, TravellerLocations, requestBuilder);
        }
        else if (RandomizerManager.Settings.DefineRefs)
        {
            for (int i = 0; i < TravellerLocations.Length; i++)
                requestBuilder.AddToVanilla(TravellerItems[i], TravellerLocations[i]);
            EditRequests(TravellerItems, TravellerLocations, requestBuilder);
        }

        if (RandomizerManager.Settings.RandomizeElderbugRewards)
        {
            for (int i = 1; i < 10; i++)
                requestBuilder.AddLocationByName($"{Elderbug_Reward_Prefix}{i}");
            requestBuilder.AddItemByName(Joker_Scroll, 3);
            requestBuilder.AddItemByName(Cleansing_Scroll);
            requestBuilder.AddItemByName(Cleansing_Scroll_Double);
            requestBuilder.AddItemByName(Lore_Page);
            requestBuilder.AddItemByName(Lore_Page_Control);

            requestBuilder.EditItemRequest(Joker_Scroll, info =>
            {
                info.getItemDef = () =>
                {
                    return new()
                    {
                        MajorItem = false,
                        Name = Joker_Scroll,
                        Pool = "Lore",
                        PriceCap = 750
                    };
                };
            });
            requestBuilder.EditItemRequest(Cleansing_Scroll, info =>
            {
                info.getItemDef = () =>
                {
                    return new()
                    {
                        MajorItem = false,
                        Name = Cleansing_Scroll,
                        Pool = "Lore",
                        PriceCap = 750
                    };
                };
            });
            requestBuilder.EditItemRequest(Cleansing_Scroll_Double, info =>
            {
                info.getItemDef = () =>
                {
                    return new()
                    {
                        MajorItem = false,
                        Name = Cleansing_Scroll_Double,
                        Pool = "Lore",
                        PriceCap = 1500
                    };
                };
            });
            requestBuilder.EditItemRequest(Lore_Page, info =>
            {
                info.getItemDef = () =>
                {
                    return new()
                    {
                        MajorItem = false,
                        Name = Lore_Page,
                        Pool = "Lore",
                        PriceCap = 300
                    };
                };
            });
            requestBuilder.EditItemRequest(Lore_Page_Control, info =>
            {
                info.getItemDef = () =>
                {
                    return new()
                    {
                        MajorItem = false,
                        Name = Lore_Page_Control,
                        Pool = "Lore",
                        PriceCap = 600
                    };
                };
            });
        }
        else if (RandomizerManager.Settings.DefineRefs)
        {
            requestBuilder.AddToVanilla(new(Wanderers_Journal, $"{Elderbug_Reward_Prefix}{1}"));
            requestBuilder.AddToVanilla(new(Hallownest_Seal, $"{Elderbug_Reward_Prefix}{2}"));
            requestBuilder.AddToVanilla(new(Lore_Page, $"{Elderbug_Reward_Prefix}{3}"));
            requestBuilder.AddToVanilla(new(Joker_Scroll, $"{Elderbug_Reward_Prefix}{4}"));
            requestBuilder.AddToVanilla(new(Lore_Page_Control, $"{Elderbug_Reward_Prefix}{5}"));
            requestBuilder.AddToVanilla(new(Joker_Scroll, $"{Elderbug_Reward_Prefix}{6}"));
            requestBuilder.AddToVanilla(new(Cleansing_Scroll, $"{Elderbug_Reward_Prefix}{7}"));
            requestBuilder.AddToVanilla(new(Joker_Scroll, $"{Elderbug_Reward_Prefix}{8}"));
            requestBuilder.AddToVanilla(new(Cleansing_Scroll_Double, $"{Elderbug_Reward_Prefix}{9}"));
        }

        if (RandomizerManager.Settings.RandomizeTreasures)
        {
            foreach (string location in TreasureLocation)
                requestBuilder.AddLocationByName(location);
            foreach (string item in TreasureItems)
                requestBuilder.AddItemByName(item);
            requestBuilder.AddLocationByName(Lemm_Door);
            requestBuilder.AddItemByName(Lemm_Order);
            EditRequests(TreasureItems, TreasureLocation, requestBuilder, true);
        }
        else if (RandomizerManager.Settings.DefineRefs)
        {
            List<AbstractItem> treasureItems = new()
            {
                Finder.GetItem(Rancid_Egg),
                Finder.GetItem(Wanderers_Journal),
                Finder.GetItem(Wanderers_Journal),
                Finder.GetItem(Wanderers_Journal),
                Finder.GetItem(Hallownest_Seal),
                Finder.GetItem(Hallownest_Seal),
                Finder.GetItem(Kings_Idol),
                Finder.GetItem(Kings_Idol),
                Finder.GetItem(Arcane_Egg),
                Finder.GetItem(Silksong_Journal),
                Finder.GetItem(Silver_Hallownest_Seal),
                Finder.GetItem(Bronze_King_Idol),
                Finder.GetItem(Golden_Arcane_Egg),
                Finder.GetItem(Magical_Key),
                Finder.GetItem(Dream_Medallion)
            };
            
            for (int i = 0; i < 14; i++)
            {
                AbstractItem rolledItem = treasureItems[requestBuilder.rng.Next(0, treasureItems.Count)];
                treasureItems.Remove(rolledItem);
                requestBuilder.AddToVanilla(rolledItem.name, TreasureLocation[i]);
            }
            requestBuilder.AddToVanilla(Lemm_Order, Lemm_Door);
        }

        if (RandomizerManager.Settings.CursedReading)
        {
            requestBuilder.AddItemByName(Read_Ability);
            requestBuilder.EditItemRequest(Read_Ability, info =>
            {
                info.getItemDef = () =>
                {
                    return new()
                    {
                        MajorItem = false,
                        Name = Read_Ability,
                        Pool = "Skill",
                        PriceCap = 1250
                    };
                };
            });
        }

        if (RandomizerManager.Settings.CursedListening)
        {
            requestBuilder.AddItemByName(Listen_Ability);
            requestBuilder.EditItemRequest(Listen_Ability, info =>
            {
                info.getItemDef = () =>
                {
                    return new()
                    {
                        MajorItem = false,
                        Name = Listen_Ability,
                        Pool = "Skill",
                        PriceCap = 1250
                    };
                };
            });
        }
    }

    private static void EditRequests(string[] items, string[] locations, RequestBuilder builder, bool treasure = false)
    {
        foreach (string item in items)
        {
            builder.EditItemRequest(item, info => 
            {
                info.getItemDef = () =>
                {
                    return new()
                    {
                        Name = item,
                        MajorItem = false,
                        Pool = treasure ? "Relics" : "Lore",
                        PriceCap = treasure ? 1000 : 1
                    };
                };
            });
        }

        foreach (string location in locations)
        {
            builder.EditLocationRequest(location, info =>
            {
                info.getLocationDef = () =>
                {
                    return new()
                    {
                        Name = location,
                        AdditionalProgressionPenalty = false,
                        FlexibleCount = false,
                        SceneName = treasure ? null : Finder.GetLocation(location).sceneName
                    };
                };
            });
        }
    }
}
