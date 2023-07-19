using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.Placements;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using LoreMaster.ItemChangerData;
using LoreMaster.ItemChangerData.Items;
using System.Collections.Generic;

namespace LoreMaster.Manager;

/// <summary>
/// Manages all IC related stuff. (Use <seealso cref="ItemList"/> and <seealso cref="LocationList"/> if you want to use the generated stuff.)
/// </summary>
public static class ItemManager
{
    #region Generating

    private static void DefineTeleporter()
    {
        Finder.DefineCustomLocation(new CoordinateLocation() { x = 35.0f, y = 5.4f, elevation = 0, sceneName = "Ruins1_27", name = "City_Teleporter" });
        Finder.DefineCustomLocation(new CoordinateLocation() { x = 57f, y = 5f, elevation = 0, sceneName = "Room_temple", name = "Temple_Teleporter" });
        Finder.DefineCustomItem(new TeleportItem()
        {
            name = "City_Ticket",
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
        Finder.DefineCustomItem(new TeleportItem()
        {
            name = "Temple_Ticket",
            UIDef = new MsgUIDef()
            {
                name = new BoxedString("Lumafly Express"),
                shopDesc = new BoxedString("If you see this, something went wrong."),
                sprite = (Finder.GetItem(ItemNames.Tram_Pass).GetResolvedUIDef() as MsgUIDef).sprite
            },
            tags = new List<Tag>()
            {
                new PersistentItemTag() { Persistence = Persistence.Persistent},
                new CompletionWeightTag() { Weight = 0 },
                new CostTag()
                {
                    Cost = new Paypal()
                }
            }
        });
    }

    #endregion

    #region Placing

    private static List<AbstractPlacement> CreateTeleporter()
    {
        List<AbstractPlacement> teleporter = new();
        MutablePlacement placement = new("City_Teleporter");
        placement.Location = Finder.GetLocation("City_Teleporter") as CoordinateLocation;
        placement.Cost = new Paypal() { ToTemple = true };
        placement.containerType = Container.Shiny;
        TeleportItem teleportItem = Finder.GetItem("City_Ticket") as TeleportItem;
        teleportItem.ToTemple = true;
        placement.Add(teleportItem);
        teleporter.Add(placement);

        placement = new("Temple_Teleporter");
        placement.Location = Finder.GetLocation("Temple_Teleporter") as CoordinateLocation;
        placement.Cost = new Paypal() { ToTemple = false };
        placement.containerType = Container.Shiny;
        teleportItem = Finder.GetItem("City_Ticket") as TeleportItem;
        teleportItem.ToTemple = false;
        placement.Add(teleportItem);
        teleporter.Add(placement);
        return teleporter;
    }

    #endregion
}
