using ItemChanger;
using ItemChanger.Items;
using ItemChanger.UIDefs;
using LoreMaster.Enums;
using LoreMaster.Helper;
using LoreMaster.ItemChangerData.Items;
using LoreMaster.ItemChangerData.Locations;
using LoreMaster.ItemChangerData.Locations.SpecialLocations;
using LoreMaster.ItemChangerData.UIDefs;
using LoreMaster.LorePowers;
using LoreMaster.Manager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LoreMaster.ItemChangerData.Other;

/// <summary>
/// A helper class to create items and locations
/// </summary>
internal static class Creator
{
    #region Items

    internal static TravellerItem CreateTravellerItem(string itemName, string key, Traveller traveller, List<Tag> tagList = null)
    {
        return new()
        {
            name = itemName,
            SoundClipName = traveller.ToString(),
            Traveller = traveller,
            UIDef = new LoreUIDef()
            {
                name = new BoxedString(traveller.ToString() + " Level"),
                sprite = new CustomSprite(traveller.ToString()),
                shopDesc = new BoxedString(Properties.ShopDescriptions.ResourceManager.GetString(traveller.ToString())),
                lore = new LanguageString(traveller switch
                {
                    Traveller.Cloth => "Minor NPC",
                    Traveller.Tiso => "Minor NPC",
                    _ => traveller.ToString()
                }, key),
                textType = TextType.LeftLore
            },
            tags = tagList
        };
    }

    internal static PowerLoreItem CreatePowerLoreItem(string itemName,
        string key,
        string sheet = "Minor NPC",
        TextType type = TextType.LeftLore,
        string soundClipName = "Lore",
        string shopDesc = null,
        List<Tag> tagList = null)
    {
        CustomSprite sprite;
        string realName = new string(itemName.SkipWhile(x => !x.Equals('-')).Skip(1).ToArray());
        shopDesc ??= Properties.ShopDescriptions.ResourceManager.GetString(realName);
        if (!string.IsNullOrEmpty(RandomizerHelper.TranslateRandoName(itemName)))
        {
            PowerManager.GetPowerByKey(RandomizerHelper.TranslateRandoName(itemName), out Power power, false);

            // Npc use their own sprites.
            if (itemName.StartsWith("Dialogue-"))
            {
                sprite = new(realName, true);
                if (!System.IO.File.Exists(System.IO.Path.GetDirectoryName(typeof(LoreMaster).Assembly.Location) + "\\Resources\\Sounds\\" + realName + ".wav"))
                    soundClipName = "Dream_Ghost";
                else
                    soundClipName = realName;
            }
            else
                sprite = new("Tablets/" + power.Location.ToString(), false);
            realName = power.PowerName;
        }
        else
        {
            // There are a few special sprites.
            if (realName.StartsWith("Shade_Golem"))
                realName = "Shade_Golem";
            else if (realName == "Grimm_Summoner_Corpse")
                realName = "Grimm_Summoner";

            if (realName == "Grimm_Machine")
                sprite = new("Lore", false);
            else
            {
                sprite = new(realName, true);
                if (soundClipName == "Lore")
                    soundClipName = realName;
            }
            realName = realName.Replace("_", " ");
        }

        return new()
        {
            name = itemName,
            loreKey = key,
            loreSheet = sheet,
            textType = type,
            UIDef = new LoreUIDef()
            {
                name = new BoxedString(realName),
                lore = new LanguageString(sheet, key),
                textType = type,
                shopDesc = new BoxedString(shopDesc),
                sprite = sprite
            },
            tags = tagList,
            SoundClipName = soundClipName
        };
    }

    internal static PowerLoreItem CreateCustomPowerLoreItem(string itemName,
        string text,
        List<Tag> tagList = null)
    {
        PowerLoreItem item = CreatePowerLoreItem(itemName, null, null, TextType.Lore, "Secret", Properties.ShopDescriptions.Fountain, tagList);
        (item.UIDef as LoreUIDef).lore = new BoxedString(text);
        return item;
    }

    internal static PowerLoreItem CreatePowerLoreItemWithDream(string itemName,
        string key,
        string sheet = "Minor NPC",
        TextType type = TextType.LeftLore,
        string soundClipName = "Lore",
        string shopDesc = null,
        List<Tag> tagList = null)
    {
        PowerLoreItem item = CreatePowerLoreItem(itemName, key, sheet, type, soundClipName, shopDesc, tagList);
        item.SoundClipName = "Dream_Enter";
        item.UIDef = CreateDreamUIDef(itemName, key, sheet, (CustomSprite)(item.UIDef as LoreUIDef).sprite);
        return item;
    }

    internal static PowerLoreItem ParseNormalLoreItem(LoreItem baseItem)
    {
        try
        {
            PowerLoreItem item = new()
            {
                name = baseItem.name + "_Empowered",
                loreKey = baseItem.loreKey,
                loreSheet = baseItem.loreSheet,
                textType = baseItem.textType,
                SoundClipName = "Lore",
                tags = baseItem.tags
            };

            PowerManager.GetPowerByKey(RandomizerHelper.TranslateRandoName(item.name), out Power power, false);
            LoreUIDef uIDef = (LoreUIDef)baseItem.UIDef;
            uIDef.sprite = new CustomSprite("Tablets/" + power.Location.ToString(), false);
            uIDef.name = new BoxedString(power.PowerName);
            item.UIDef = uIDef;
            return item;
        }
        catch (System.Exception exception)
        {
            LoreMaster.Instance.LogError(exception.Message + "\n" + exception.StackTrace);
            return null;
        }
    }

    #endregion

    #region Locations

    public static DreamNailLocation CreateDreamImpactLocation(string locationName, string scene, string gameObjectName = "/Dream Dialogue")
    {
        if (locationName == LocationList.Tiso_Corpse)
            return new TisoCorpseLocation()
            {
                name = locationName,
                sceneName = scene,
                flingType = FlingType.DirectDeposit,
                GameObjectName = gameObjectName,
                tags = new()
                {
                    ItemManager.CreateInteropTag(scene, locationName)
                }
            };
        else
            return new()
            {
                name = locationName,
                sceneName = scene,
                flingType = FlingType.DirectDeposit,
                GameObjectName = gameObjectName,
                tags = new()
                {
                    ItemManager.CreateInteropTag(scene, locationName)
                }
            };
    }

    internal static InspectLocation CreateInspectLocation(string locationName, string scene, Vector3 position)
    {
        return new()
        {
            name = locationName,
            flingType = FlingType.DirectDeposit,
            forceShiny = false,
            Position = position,
            sceneName = scene,
            tags = new()
                {
                    ItemManager.CreateInteropTag(scene, locationName)
                }
        };
    }

    internal static InspectLocation CreateInspectLocation(string locationName, string scene, string gameObjectName)
    {
        return new()
        {
            name = locationName,
            flingType = FlingType.DirectDeposit,
            forceShiny = false,
            GameObjectName = gameObjectName,
            sceneName = scene,
            tags = new()
                {
                    ItemManager.CreateInteropTag(scene, locationName)
                }
        };
    }

    /// <summary>
    /// Creates a npc locations. This method is used, because the deserialization of IC calls the default constructor (or another constructor, but with <see langword="null"/> values.
    /// </summary>
    public static DialogueLocation CreateDialogueLocation(string locationName, string scene, string objectName, string fsmName = "Conversation Control")
        => new()
        {
            name = locationName,
            sceneName = scene,
            ObjectName = objectName,
            FsmName = fsmName,
            tags = new()
                {
                    ItemManager.CreateInteropTag(scene, locationName)
                }
        };

    #endregion

    #region UI Def

    internal static DreamLoreUIDef CreateDreamUIDef(string itemName, string key, string sheet, CustomSprite customSprite)
    {
        return new()
        {
            // Cut "Dream_Dialogue-" out of the name
            name = new BoxedString(itemName.Substring(15).Replace("_", " ")),
            Key = key,
            Sheet = sheet,
            shopDesc = new BoxedString("The last words, they spoke. Radiance whispered me that... ehm, I mean I took a dream nail myself."),
            sprite = customSprite
        };
    }

    #endregion
}
