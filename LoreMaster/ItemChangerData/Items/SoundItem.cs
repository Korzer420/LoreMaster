using ItemChanger;
using LoreMaster.Manager;
using System.Linq;

namespace LoreMaster.ItemChangerData.Items;

/// <summary>
/// An item which plays a sound upon pick up. (Unless a lore or the item screen is prompted)
/// </summary>
internal class SoundItem : AbstractItem
{
    /// <summary>
    /// Gets or sets the name of the clip.
    /// </summary>
    public string ClipName { get; set; }

    public override void GiveImmediate(GiveInfo info)
    {
        if (info.MessageType != MessageType.Big
            && !string.IsNullOrEmpty(ClipName))
            SoundEffectManager.Manager.PlayClipAtPoint(ClipName, HeroController.instance.transform.position);
    }

    public override AbstractItem Clone()
    {
        return new SoundItem()
        {
            name = name,
            tags = tags?.Select((Tag t) => t.Clone())?.ToList(),
            UIDef = UIDef.Clone(),
            ClipName = ClipName
        };
    }
}
