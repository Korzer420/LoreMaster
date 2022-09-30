using ItemChanger;
using ItemChanger.Components;
using ItemChanger.Items;
using ItemChanger.Locations;
using ItemChanger.Placements;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using LoreMaster.CustomItem;
using LoreMaster.CustomItem.Locations;
using LoreMaster.LorePowers.CityOfTears;
using LoreMaster.Randomizer.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LoreMaster.Manager;

internal static class ItemManager
{
    internal static List<AbstractPlacement> CustomPlacements { get; set; } = new();

    internal static void DefineItems()
    {
        ItemChangerMod.CreateSettingsProfile(false);

    }

    internal static void CreateCustomItems()
    {
        ItemChangerMod.CreateSettingsProfile(false);

        // Add items for the black egg temple teleporter.
        MutablePlacement teleportPlacement = new CoordinateLocation() { x = 35.0f, y = 5.4f, elevation = 0, sceneName = "Ruins1_27", name = "City_Teleporter" }.Wrap() as MutablePlacement;
        teleportPlacement.Cost = new Paypal { ToTemple = true };
        teleportPlacement.Add(new TouristMagnetItem("City_Teleporter"));
        CustomPlacements.Add(teleportPlacement);

        MutablePlacement secondPlacement = new CoordinateLocation() { x = 57f, y = 5f, elevation = 0, sceneName = "Room_temple", name = "Temple_Teleporter" }.Wrap() as MutablePlacement;
        secondPlacement.Cost = new Paypal { ToTemple = false };
        secondPlacement.Add(new TouristMagnetItem("Temple_Teleporter"));
        CustomPlacements.Add(secondPlacement);

        // Treasure Hunter stuff
        try
        {
            LemmSignLocation lemmSignLocation = new()
            {
                sceneName = "Ruins1_05b",
                name = "Lemm_Door",
                flingType = FlingType.DirectDeposit
            };
            AbstractPlacement lemmPlacement = lemmSignLocation.Wrap();
            lemmPlacement.Items.Add(NpcItem.CreateItem("Lemm_Sign", "RELICDEALER_DOOR", "I didn't steal this from the door while Lemm was away... just to be clear.", "Lore", "Relic Dealer"));
            lemmPlacement.Items.Add(new BoolItem()
            {
                name = "Lemm_Order",
                fieldName = "lemm_Allow",
                setValue = true,
                UIDef = new MsgUIDef()
                {
                    name = new BoxedString("Lemm's Order"),
                    shopDesc = new BoxedString("The order of the relic seeker for a few treasure charts from Cornifer. But it seems like Lemm hasn't pay the price yet..."),
                    sprite = new CustomSprite("Lemms_Order", false)
                }
            });
            CustomPlacements.Add(lemmPlacement);

            // Charts
            ShopLocation iselda2 = Finder.GetLocation("Iselda_Treasure") as ShopLocation;
            if (iselda2 == null)
            {
                iselda2 = new()
                {
                    name = "Iselda_Treasure",
                    sceneName = "Room_mapper",
                    requiredPlayerDataBool = "lemm_allow",
                    fsmName = "Conversation Control",
                    objectName = "Iselda",
                    flingType = FlingType.DirectDeposit
                };
                Finder.DefineCustomLocation(iselda2);
            }

            ShopPlacement shopPlacement = (ShopPlacement)iselda2.Wrap();
            shopPlacement.defaultShopItems = DefaultShopItems.IseldaCharms | DefaultShopItems.IseldaMaps
                  | DefaultShopItems.IseldaMapMarkers | DefaultShopItems.IseldaMapPins | DefaultShopItems.IseldaQuill;

            CustomPlacements.Add(shopPlacement);
            List<int> chartPrices = new() { 1, 30, 69, 120, 160, 200, 230, 290, 420, 500, 750, 870, 1000, 1150 };
            for (int i = 1; i < 15; i++)
            {
                int rolledPrice = chartPrices[LoreMaster.Instance.Generator.Next(0, chartPrices.Count)];
                chartPrices.Remove(rolledPrice);
                BoolItem item = new()
                {
                    fieldName = "Treasure_Chart_" + i,
                    name = "Treasure_Chart_" + i,
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
                    new CostTag()
                    {
                         Cost = new GeoCost(rolledPrice)
                    }
                }
                };
                shopPlacement.Items.Add(item);
            }

            List<AbstractItem> availableTreasures = TreasureHunterPower.GetTreasureItems();
            // Place the treasures
            for (int i = 0; i < 14; i++)
                if (Finder.GetLocation("Treasure_" + (i + 1)) == null)
                {
                    TreasureLocation treasureLocation = TreasureLocation.GenerateLocation(i);
                    AbstractPlacement abstractPlacement = treasureLocation.Wrap();
                    abstractPlacement.Items.Add(availableTreasures[i]);
                    CustomPlacements.Add(abstractPlacement);
                    Finder.DefineCustomLocation(treasureLocation);
                }
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Error while setting up treasure charts: " + exception.Message);
            LoreMaster.Instance.LogError(exception.StackTrace);
        }

        StagEggLocation stagEggLocation = new()
        {
            name = "Stag_Nest",
            sceneName = "Cliffs_03",
            flingType = FlingType.DirectDeposit
        };

        AbstractPlacement stagPlacement = stagEggLocation.Wrap();
        stagPlacement.Add(NpcItem.CreateItem("Stag_Egg_Inspect", "STAG_EGG_INSPECT", "See this picture of an egg I took.", "Stag_Egg", "Stag"));
        stagPlacement.Add(new BoolItem()
        {
            fieldName = "hasStagEgg",
            name = "Stag_Egg",
            setValue = true,
            UIDef = new MsgUIDef()
            {
                name = new BoxedString("Stag Egg"),
                shopDesc = new BoxedString("If you are seeing this, something went wrong."),
                sprite = new CustomSprite("Stag_Egg", false)
            }
        });
        ZoteLocation zote = new()
        {
            sceneName = "Deepnest_33",
            flingType = FlingType.DirectDeposit,
            name = "Zote_Deepnest"
        };
        CustomPlacements.AddRange(new AbstractPlacement[] { stagPlacement, zote.Wrap() });

        if (SettingManager.Instance.GameMode != Enums.GameMode.Normal)
        {
            AbstractItem[] elderbugRewards = new AbstractItem[]
            {
                new BoolItem()
                {
                    fieldName = "Read",
                    name = "Read_Ability",
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
                },
                new BoolItem()
                {
                    fieldName = "Listen",
                    name = "Listen_Ability",
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
                },
                new BoolItem()
                {
                    name = "Lore_Page",
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
                },
                new IntItem()
                {
                    amount = 1,
                    fieldName = "Joker_Scroll",
                    name = "Joker_Scroll",
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
                },
                new BoolItem()
                {
                    name = "Lore_Page_Control",
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
                },
                new CleansingScrollItem()
                {
                    amount = 1,
                    fieldName = "CleansingScroll",
                    name = "Cleansing_Scroll",
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
                },
                new IntItem()
                {
                    amount = 1,
                    fieldName = "Joker_Scroll",
                    name = "Joker_Scroll",
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
                },
                new CleansingScrollItem()
                {
                    amount = 2,
                    fieldName = "CleansingScroll",
                    name = "Cleansing_Scroll_Double",
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
                },
                new IntItem()
                {
                    amount = 1,
                    fieldName = "Joker_Scroll",
                    name = "Joker_Scroll",
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
                },
            };
            for (int i = 0; i < elderbugRewards.Length; i++)
            {
                ElderbugLocation elderbugLocation = new()
                {
                    sceneName = "Town",
                    name = $"Elderbug_Reward_{i + 1}",
                    flingType = FlingType.Everywhere
                };
                AbstractPlacement placement = elderbugLocation.Wrap();
                placement.Items.Add(elderbugRewards[i]);
                CustomPlacements.Add(placement);
            }
        }

        PathOfPainLocation pop = new()
        {
            flingType = FlingType.DirectDeposit,
            name = "Path_of_Pain-End_Scene",
            sceneName = "White_Palace_20",
        };
        AbstractPlacement PoP = pop.Wrap();
        PoP.Items.Add(new BoolItem()
        {
            fieldName = "PopLore",
            name = "Lore_Tablet-Path_of_Pain_Reward",
            setValue = true,
            UIDef = new MsgUIDef()
            { 
                name = new BoxedString("Sacred Shell"),
                shopDesc = new BoxedString("This is a secret, that the Pale King tried to hide as best as he can."),
                sprite = new CustomSprite("Tablets/WhitePalace", false)
            }
        });
        CustomPlacements.Add(PoP);

        RecordBelaLocation bela = new()
        {
            sceneName = "Ruins1_30",
            flingType = FlingType.DirectDeposit,
            name = "Record_Bela",
        };
        AbstractPlacement currentPlacement = bela.Wrap();
        currentPlacement.Items.Add(NpcItem.CreateItem("Lore_Tablet_Sanctum_Spell_Twister", "MAGE_COMP_02",
            "A hidden lore tablet deep within the Soul Sanctum.", "Tablets/CityOfTears", "Lore Tablets", false));
        CustomPlacements.Add(currentPlacement);
        ItemChangerMod.AddPlacements(CustomPlacements);
    }

    /// <summary>
    /// Reset all items, so that they can be obtained again.
    /// </summary>
    internal static void ResetItems()
    {
        CustomPlacements.ForEach(x => x.Items.ForEach(x => x.RefreshObtained()));
    }

    internal static T GetLocationByName<T>(string name) where T : AbstractLocation =>
        ((AutoPlacement)CustomPlacements.FirstOrDefault(x => string.Equals(x.Name, name))).Location as T;
}
