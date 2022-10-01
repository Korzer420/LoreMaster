using ItemChanger;
using ItemChanger.Items;
using ItemChanger.UIDefs;
using LoreMaster.ItemChanger;

namespace LoreMaster.Randomizer.Items;

internal class NpcItem : LoreItem
{
    public NpcItem() { }

    public static NpcItem CreateItem(string itemName, string key, string shopDescription, string sprite = "Lore", string sheet = "Minor NPC", bool isRandoSprite = true)
    {
        return new()
        {
            name = itemName.StartsWith("Lore_Tablet-") ? itemName : "Lore_Tablet-" + itemName,
            UIDef = new LoreUIDef()
            {
                name = new BoxedString(itemName?.Replace('_', ' ')),
                lore = new BoxedString("You shouldn't be able to see this."),
                shopDesc = new BoxedString(shopDescription),
                sprite = new CustomSprite(sprite, isRandoSprite)
            },
            loreKey = key,
            loreSheet = sheet
        };
    }

    public override void GiveImmediate(GiveInfo info)
    {
        ((LoreUIDef)UIDef).lore = new BoxedString(Language.Language.Get(loreKey, loreSheet));
        base.GiveImmediate(info);
    }
}
