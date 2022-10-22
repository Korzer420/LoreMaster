using ItemChanger;
using ItemChanger.Items;
using ItemChanger.UIDefs;
using LoreMaster.Helper;
using LoreMaster.LorePowers;
using LoreMaster.Manager;

namespace LoreMaster.ItemChangerData.Items;

/// <summary>
/// An lore item which adds itself to the obtained powers and activates it. Also plays a custom sound file.
/// </summary>
internal class PowerLoreItem : LoreItem
{
    /// <summary>
    /// Gets or sets the name of the sound file which should be played.
    /// </summary>
    public string SoundClipName { get; set; } = "Lore";

    public override void GiveImmediate(GiveInfo info)
    {
        // Check if item is an actual power.
        string tabletName = RandomizerHelper.TranslateRandoName(name);
        if (!string.IsNullOrEmpty(tabletName))
        {
            PowerManager.GetPowerByKey(tabletName, out Power power);
            if (UIDef is LoreUIDef uiDef)
                uiDef.lore = new BoxedString(LoreManager.Instance.AddPowerData(power, uiDef.lore.Value, tabletName == "PLAQUE_WARN"));
        }
        else
            PowerManager.ObtainedPowers.Add(new PlaceholderPower());

        // Plays the given sound file.
        if (SoundClipName == "Lore")
            base.GiveImmediate(info);
        else
            SoundEffectManager.Manager.PlayClipAtPoint(SoundClipName, HeroController.instance.transform.position);
        LoreMaster.Instance.Log("Gifted item is: " + name);
    }
}
