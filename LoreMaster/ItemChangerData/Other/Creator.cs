using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using LoreMaster.Enums;
using LoreMaster.ItemChangerData.Items;
using LoreMaster.ItemChangerData.Locations;
using LoreMaster.ItemChangerData.Locations.SpecialLocations;
using LoreMaster.ItemChangerData.UIDefs;
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

    internal static SoundItem CreateSoundItem(string itemName, string clipName, UIDef uIDef, List<Tag> tagList = null)
    {
        return new()
        {
            name = itemName,
            ClipName = clipName,
            UIDef = uIDef,
            tags = tagList
        };
    }

    internal static TravellerItem CreateTravellerItem(string itemName, string key, Traveller traveller, List<Tag> tagList = null)
    {
        return new()
        {
            name = itemName,
            ClipName = traveller.ToString(),
            Traveller = traveller,
            UIDef = new LoreUIDef()
            {
                name = new BoxedString(traveller.ToString()),
                sprite = new CustomSprite(traveller.ToString()),
                shopDesc = new BoxedString(Properties.ShopDescriptions.ResourceManager.GetString(traveller.ToString())),
                lore = new LanguageString(traveller switch
                {
                    Traveller.Cloth => "Minor NPC",
                    Traveller.Tiso => "Minor NPC",
                    _ => traveller.ToString()
                }, key),
                textType = TextType.Lore
            },
            tags = tagList
        };
    }

    internal static PowerLoreItem CreatePowerLoreItem(string itemName, string key, string sheet, TextType type, UIDef uIDef, List<Tag> tagList = null)
    {
        return new()
        {
            name = itemName,
            loreKey = key,
            loreSheet = sheet,
            textType = type,
            UIDef = uIDef,
            tags = tagList
        };
    }

    internal static NpcItem CreateNpcItem(string itemName, string key, string sheet = "Minor NPC", List<Tag> tagList = null)
    {
        string npcName = new string(itemName.SkipWhile(x => x != '-').Skip(1).ToArray());
        return new()
        {
            name = itemName,
            textType = TextType.LeftLore,
            loreKey = key,
            loreSheet = sheet,
            UIDef = new LoreUIDef()
            {
                name = new BoxedString(npcName.Replace("_", " ")),
                shopDesc = new BoxedString(Properties.ShopDescriptions.ResourceManager.GetString(npcName)),
                sprite = new CustomSprite(npcName.EndsWith("_Diary") ? npcName.Substring(0,npcName.Length - 6) : npcName),
                textType = TextType.LeftLore,
                lore = new LanguageString(sheet,key)
            },
            tags = tagList
        };
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
                GameObjectName = gameObjectName
            };
        else
            return new()
            {
                name = locationName,
                sceneName = scene,
                flingType = FlingType.DirectDeposit,
                GameObjectName = gameObjectName
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
            sceneName = scene
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
            sceneName = scene
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
