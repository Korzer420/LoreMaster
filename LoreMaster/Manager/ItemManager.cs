using ItemChanger;
using ItemChanger.Items;
using ItemChanger.Locations;
using ItemChanger.Placements;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using LoreMaster.CustomItem;
using LoreMaster.LorePowers.CityOfTears;
using LoreMaster.Randomizer.Items;
using System;
using System.Collections.Generic;

namespace LoreMaster.Manager
{
    internal class ItemManager
    {
        internal static void CreateCustomItems()
        {
            // Add items for the black egg temple teleporter.
            ItemChangerMod.CreateSettingsProfile(false);
            List<MutablePlacement> teleportItems = new();
            MutablePlacement teleportPlacement = new CoordinateLocation() { x = 35.0f, y = 5.4f, elevation = 0, sceneName = "Ruins1_27", name = "City_Teleporter" }.Wrap() as MutablePlacement;
            teleportPlacement.Cost = new Paypal { ToTemple = true };
            teleportPlacement.Add(new TouristMagnetItem("City_Teleporter"));
            teleportItems.Add(teleportPlacement);

            MutablePlacement secondPlacement = new CoordinateLocation() { x = 57f, y = 5f, elevation = 0, sceneName = "Room_temple", name = "Temple_Teleporter" }.Wrap() as MutablePlacement;
            secondPlacement.Cost = new Paypal { ToTemple = false };
            secondPlacement.Add(new TouristMagnetItem("Temple_Teleporter"));
            teleportItems.Add(secondPlacement);
            ItemChangerMod.AddPlacements(teleportItems);

            // Treasure Hunter stuff
            try
            {
                List<AbstractPlacement> treasurePlacements = new();

                LemmSignLocation lemmSignLocation = new()
                {
                    sceneName = "Ruins1_05b",
                    name = "Lemm_Door",
                    flingType = FlingType.DirectDeposit
                };
                AbstractPlacement lemmPlacement = lemmSignLocation.Wrap();
                lemmPlacement.Items.Add(NpcItem.CreateItem("Lemm_Sign", "RELICDEALER_DOOR", "Don't ask why this is here... please.", "Lore", "Relic Dealer"));
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
                treasurePlacements.Add(lemmPlacement);

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
                treasurePlacements.Add(shopPlacement);

                List<int> chartPrices = new() { 1, 30, 69, 120, 160, 200, 230, 290, 420, 500, 750, 870, 1000, 1150 };
                for (int i = 1; i < 15; i++)
                {
                    int rolledPrice = chartPrices[LoreMaster.Instance.Generator.Next(0, chartPrices.Count)];
                    chartPrices.Remove(rolledPrice);
                    BoolItem item = new()
                    {
                        fieldName = "Treasure_Chart_" + i,
                        name = "Treasure Chart " + i,
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
                        treasurePlacements.Add(abstractPlacement);
                    }
                ItemChangerMod.AddPlacements(treasurePlacements);
            }
            catch (Exception exception)
            {
                LoreMaster.Instance.LogError("Error while setting up treasure charts: " + exception.Message);
                LoreMaster.Instance.LogError("Error while setting up treasure charts: " + exception.StackTrace);
            }
        }
    }
}
