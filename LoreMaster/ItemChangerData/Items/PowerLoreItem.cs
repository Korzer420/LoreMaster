using ItemChanger;
using ItemChanger.Items;
using ItemChanger.UIDefs;
using System.Linq;

namespace LoreMaster.ItemChangerData.Items;

/// <summary>
/// An lore item which resolves it's text later, to obtain a power that is linked to it.
/// </summary>
internal class PowerLoreItem : LoreItem
{
    public override void GiveImmediate(GiveInfo info)
    {
        if (UIDef is LoreUIDef lore)
            lore.lore = new BoxedString(Language.Language.Get(loreKey, loreSheet));
        else if (UIDef is BigUIDef screen)
            screen.descTwo = new BoxedString(Language.Language.Get(loreKey, loreSheet));
        // Npc items display their own sound instead of the normal one.
        if (this is not NpcItem)
            base.GiveImmediate(info);
    }

    public override AbstractItem Clone()
    {
        return new PowerLoreItem()
        {
            name = name,
            loreKey = loreKey,
            loreSheet = loreSheet,
            tags = tags?.Select((Tag t) => t.Clone())?.ToList(),
            textType = textType,
            UIDef = UIDef.Clone()
        };
    }
}
