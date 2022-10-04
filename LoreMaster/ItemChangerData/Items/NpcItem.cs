using ItemChanger;
using LoreMaster.Manager;
using System.Linq;

namespace LoreMaster.ItemChangerData.Items;

/// <summary>
/// A stitched version from a power lore and sound item. Used for npc's
/// </summary>
internal class NpcItem : PowerLoreItem
{
    public override void GiveImmediate(GiveInfo info)
    {
        string clipName = new string(name.SkipWhile(x => x != '-').Skip(1).ToArray());
        if (info.MessageType != MessageType.Big)
        {
            // Marissa and the dream warrior have their own sound, every other ghost uses the default spawn sound.
            if (loreSheet == "Ghosts" && clipName != "Marissa" && !name.StartsWith("Inspect"))
                SoundEffectManager.Manager.PlayClipAtPoint("Dream_Ghost", HeroController.instance.transform.position);
            else if (clipName != "Godseeker")
                SoundEffectManager.Manager.PlayClipAtPoint(clipName, HeroController.instance.transform.position);
        }
        base.GiveImmediate(info);
    }

    public override AbstractItem Clone()
    {
        return new NpcItem()
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
