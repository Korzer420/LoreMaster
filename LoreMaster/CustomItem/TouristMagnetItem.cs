using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using System.Collections.Generic;

namespace LoreMaster.CustomItem;

public class TouristMagnetItem : AbstractItem
{
    private const string _name = "Lumafly Express";

    public TouristMagnetItem(string itemName)
    {
        name = itemName;
        UIDef = new MsgUIDef()
        {
            name = new BoxedString(_name),
            shopDesc = new BoxedString(_name),
            sprite = new CustomSprite()
        };
        tags = new List<Tag>()
        {
            new PersistentItemTag() { Persistence = Persistence.Persistent},
            new CompletionWeightTag() { Weight = 0}
        };
    }

    public override void GiveImmediate(GiveInfo info) { }
}
