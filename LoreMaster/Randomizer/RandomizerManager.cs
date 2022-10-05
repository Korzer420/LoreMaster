using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.UIDefs;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using LoreMaster.Manager;
using LoreMaster.Randomizer.Items;
using LoreMaster.SaveManagement;
using System.Collections.Generic;
using RandomizerMod.Extensions;
using ItemChanger.Internal;
using System.Linq;
using LoreMaster.LorePowers.CityOfTears;
using LoreMaster.LorePowers.RestingGrounds;
using LoreMaster.LorePowers;
using ItemChanger.Tags;
using Modding;
using LoreMaster.ItemChangerData.Locations;
using LoreMaster.ItemChangerData.Items;
using LoreMaster.ItemChangerData.Other;

namespace LoreMaster.Randomizer;

public static class RandomizerManager
{
    #region Members

    private static RandomizerSettings _settings;

    private static Dictionary<Area, string> _randoAreaNames = new()
    {
        {Area.None, "Menu" },
        {Area.AncientBasin, "Ancient Basin" },
        {Area.CityOfTears, "City of Tears" },
        {Area.Dirtmouth, "Dirtmouth"},
        {Area.Crossroads, "Forgotten Crossroads"},
        {Area.Greenpath, "Greenpath"},
        {Area.Deepnest, "Deepnest" },
        {Area.FungalWastes, "Fungal Wastes"},
        {Area.QueensGarden, "Queen's Gardens"},
        {Area.Peaks, "Crystal Peaks"},
        {Area.RestingGrounds, "Resting Grounds"},
        {Area.Waterways, "Royal Waterways"},
        {Area.KingdomsEdge, "Kingdom's Edge"},
        {Area.FogCanyon, "Fog Canyon"},
        {Area.Cliffs, "Howling Cliffs"},
        {Area.WhitePalace, "White Palace"},
    };

    #endregion

    #region Properties

    /// <summary>
    /// Gets the settings of a randomizer.
    /// </summary>
    public static RandomizerSettings Settings => _settings ??= new();

    public static List<string> NpcItemNames => new()
    {
        "Bretta",
        "Elderbug",
        "Bardoon",
        "Vespa",
        "Mask_Maker",
        "Midwife",
        "Gravedigger",
        "Poggy",
        "Joni",
        "Myla",
        "Emilitia",
        "Willoh",
        "Moss_Prophet",
        "Fluke_Hermit",
        "Quirrel",
        "Grasshopper",
        "Marissa",
        "Queen"
    };

    public static List<string> StatueItemNames => new()
    {
        "Xero",
        "Elder_Hu",
        "Gorb",
        "Galien",
        "Marmu",
        "Markoth",
        "No_Eyes"
    };

    

    /// <summary>
    /// Gets the flag, that indicates if this is a rando file.
    /// </summary>
    public static bool PlayingRandomizer
    {
        get
        {
            if (ModHooks.GetMod("Randomizer 4", true) is not Mod)
                return false;
            else
                return RandoFile;
        }
    }

    /// <summary>
    /// Gets the flag, that indicates if this is a rando file. To prevent missing reference exceptions, this is seperated from <see cref="PlayingRandomizer"/>.
    /// </summary>
    private static bool RandoFile => RandomizerMod.RandomizerMod.IsRandoSave;

    #endregion

    #region Methods

    internal static void LoadSettings(LoreMasterGlobalSaveData saveData)
    {
        _settings = saveData.RandoSettings;
    }

    /// <summary>
    /// Attach the lore master to randomizer
    /// </summary>
    public static void AttachToRandomizer()
    {
        RandomizerMenu.AttachMenu();
        RandomizerRequestModifier.ModifyRequest();
        LogicManager.AttachLogic();
        RandomizerMod.RC.RandoController.OnCalculateHash += RandoController_OnCalculateHash;
    }

    private static int RandoController_OnCalculateHash(RandomizerMod.RC.RandoController controller, int original)
    {
        if (Settings.PowerBehaviour != LoreSetOption.Default || Settings.BlackEggTempleCondition != BlackEggTempleCondition.Dreamers)
            return 72767 + PowerManager.GetAllPowers().Count() + ((int)Settings.PowerBehaviour * 120 + (int)Settings.BlackEggTempleCondition * Settings.NeededLore);
        return 0;
    }

    internal static void DefineItems()
    {
        if (Settings.RandomizeNpc)
        {
            if (Finder.GetItem("Lore_Tablet-Bretta") == null)
            {
                

                // The lore page will also be randomized, since elderbug cannot be talked to.
                Finder.DefineCustomItem(new AbilityItem()
                {
                    name = "Lore_Page",
                    Item = Enums.CustomItemType.LoreControl,
                    UIDef = new BigUIDef()
                    {
                        name = new BoxedString("Lore Page"),
                        shopDesc = new BoxedString("This will be very helpful. Trust me on this one."),
                        sprite = (Finder.GetItem("Hunter's_Journal").UIDef.Clone() as BigUIDef).sprite,
                        descOne = new BoxedString("You can now sense which knowledge you acquired."),
                        descTwo = new BoxedString("Additionally, you can toggle on/off unwanted powers while you rest."),
                        take = new BoxedString("You got:"),
                        bigSprite = (Finder.GetItem("Hunter's_Journal").UIDef.Clone() as BigUIDef).bigSprite
                    },
                    boolName = "Unused", // The bool logic gets executed seperately. This is just for avoiding Serialization errors.
                    moduleName = "Unused" // Same as boolName.
                });
            }

            if (Finder.GetLocation("Gravedigger_Dialogue") == null)
            {
                
                Finder.DefineCustomLocation(new AbilityLocation()
                {
                    forceShiny = true,
                    sceneName = "Town",
                    name = "Town_Lore_Page",
                    x = 107.61f,
                    y = 11.41f
                });
            }
        }

        if (Settings.RandomizeWarriorStatues && Finder.GetItem("Lore_Tablet-Xero") == null)
        {
            //Finder.DefineCustomItem(Creator.CreateItem("Xero", "XERO_INSPECT", "The words of the king, for what happens with traitors.", "Xero", "Ghosts"));
            //Finder.DefineCustomItem(NpcItem.CreateItem("Elder_Hu", "HU_INSPECT", "The mantis buried him even after he attacked them. How nice of them.", "Elder_Hu", "Ghosts"));
            //Finder.DefineCustomItem(NpcItem.CreateItem("Galien", "GALIEN_INSPECT", "Something from an idiot who thought it would be a good idea to an area full a beasts. And then he died... \"Insert Surprised Pikachu Meme here\"", "Galien", "Ghosts"));
            //Finder.DefineCustomItem(NpcItem.CreateItem("Marmu", "MUMCAT_INSPECT", "They say the lore master creator hates this damn *****. Whatever that means.", "Marmu", "Ghosts"));
            //Finder.DefineCustomItem(NpcItem.CreateItem("Gorb", "ALADAR_INSPECT", "It's gorbin time! (to ascent!) (with Gorb) <br>- Gorb", "Gorb", "Ghosts"));
            //Finder.DefineCustomItem(NpcItem.CreateItem("Markoth", "MARKOTH_INSPECT", "Did the shade gate exists before he went in? If so, HOW could've he pass that? Is Markoth void? O.o", "Markoth", "Ghosts"));
            //Finder.DefineCustomItem(NpcItem.CreateItem("No_Eyes", "NOEYES_INSPECT", "I wonder what her name was before she... you know. If that's her real name, that is just asking for something to rip out your eyes.", "No_Eyes", "Ghosts"));

            //Finder.DefineCustomLocation(TabletLocation.CreateLocation("Xero", "RestingGrounds_02"));
            //Finder.DefineCustomLocation(TabletLocation.CreateLocation("Elder_Hu", "Fungus2_32"));
            //Finder.DefineCustomLocation(TabletLocation.CreateLocation("Galien", "Deepnest_40"));
            //Finder.DefineCustomLocation(TabletLocation.CreateLocation("Marmu", "Fungus3_40"));
            //Finder.DefineCustomLocation(TabletLocation.CreateLocation("Gorb", "Cliffs_02"));
            //Finder.DefineCustomLocation(TabletLocation.CreateLocation("Markoth", "Deepnest_East_10"));
            //Finder.DefineCustomLocation(TabletLocation.CreateLocation("No_Eyes", "Fungus1_35"));
        }

        if (Settings.CursedReading && Finder.GetItem("Reading") == null)
        {
            LoreManager.Instance.CanRead = false;
            Finder.DefineCustomItem(new AbilityItem()
            {
                name = "Reading",
                Item = CustomItemType.Reading,
                UIDef = new BigUIDef()
                {
                    name = new BoxedString("Reading"),
                    shopDesc = new BoxedString("This will be very helpful. Trust me on this one."),
                    sprite = (Finder.GetItem("World_Sense").UIDef.Clone() as BigUIDef).sprite,
                    descOne = new BoxedString("You finally learnt how to read!"),
                    descTwo = new BoxedString("You can now comprehend the knowledge written down in the kingdom."),
                    take = new BoxedString("You learnt:"),
                    bigSprite = (Finder.GetItem("World_Sense").UIDef.Clone() as BigUIDef).bigSprite
                },
                boolName = "Reading", // The bool logic gets executed seperately. This is just for avoiding Serialization errors.
                moduleName = "Unused", // Same as boolName.
                tags = new()
                {
                    new InteropTag()
                    {
                        Message = "RandoSupplementalMetadata",
                        Properties = new()
                        {
                            {"ModSource", "LoreMaster" },
                            {"PoolGroup", "Skills" }
                        }
                    }
                }
            });
            Finder.DefineCustomLocation(new AbilityLocation()
            {
                forceShiny = true,
                sceneName = "Town",
                name = "Town_Read",
                x = 115.61f,
                y = 11.41f
            });
        }

        if (Settings.CursedListening && Finder.GetItem("Listening") == null)
        {
            LoreManager.Instance.CanListen = false;
            Finder.DefineCustomItem(new AbilityItem()
            {
                name = "Listening",
                Item = CustomItemType.Listening,
                UIDef = new BigUIDef()
                {
                    name = new BoxedString("Listening"),
                    shopDesc = new BoxedString("You should not be able to see this... I mean, you can't understand me."),
                    sprite = (Finder.GetItem("World_Sense").UIDef.Clone() as BigUIDef).sprite,
                    take = new BoxedString("You learnt:"),
                    descOne = new BoxedString("You finally learnt how to listen!"),
                    descTwo = new BoxedString("You can now \"communicate\" with the residents of Hallownest."),
                    bigSprite = (Finder.GetItem("World_Sense").UIDef.Clone() as BigUIDef).bigSprite
                },
                boolName = "Listening", // The bool logic gets executed seperately. This is just for avoiding Serialization errors.
                moduleName = "Unused", // Same as boolName.
                tags = new()
                {
                    new InteropTag()
                    {
                        Message = "RandoSupplementalMetadata",
                        Properties = new()
                        {
                            {"ModSource", "LoreMaster" },
                            {"PoolGroup", "Skills" }
                        }
                    }
                }
            });
            Finder.DefineCustomLocation(new AbilityLocation()
            {
                forceShiny = true,
                sceneName = "Town",
                name = "Town_Listen",
                x = 111.62f,
                y = 11.41f
            });
        }
    }

    internal static LoreSetOption CheckForRandoFile()
    {
        if (!RandomizerMod.RandomizerMod.IsRandoSave)
        {
            LoreManager.Instance.CanListen = SettingManager.Instance.GameMode == GameMode.Normal;
            LoreManager.Instance.CanRead = SettingManager.Instance.GameMode == GameMode.Normal;
            return LoreSetOption.Default;
        }
        else
            return Settings.PowerBehaviour;
    }

    internal static bool RandoTracker(Area area, out string areaLore)
    {
        areaLore = null;
        if (!RandomizerMod.RandomizerMod.IsRandoSave || !RandomizerMod.RandomizerMod.RS.GenerationSettings.PoolSettings.LoreTablets)
            return false;
        int maxPowerAmount = 0;
        int collectedPowerAmount = 0;

        if (!Settings.RandomizeNpc)
        {
            Power power = null;
            switch (area)
            {
                case Area.Dirtmouth:
                    maxPowerAmount += 2;
                    if (PowerManager.GetPowerByKey("GRAVEDIGGER", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    if (PowerManager.GetPowerByKey("BRETTA", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.Crossroads:
                    maxPowerAmount++;
                    if (PowerManager.GetPowerByKey("MYLA", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.Cliffs:
                    maxPowerAmount++;
                    if (PowerManager.GetPowerByKey("JONI", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.FungalWastes:
                    maxPowerAmount++;
                    if (PowerManager.GetPowerByKey("WILLOH", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.CityOfTears:
                    maxPowerAmount += 3;
                    if (PowerManager.GetPowerByKey("POGGY", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    if (PowerManager.GetPowerByKey("MARISSA", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    if (PowerManager.GetPowerByKey("EMILITIA", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.Waterways:
                    maxPowerAmount++;
                    if (PowerManager.GetPowerByKey("FLUKE_HERMIT", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.Deepnest:
                    maxPowerAmount += 2;
                    if (PowerManager.GetPowerByKey("MIDWIFE", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    if (PowerManager.GetPowerByKey("MASKMAKER", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.QueensGarden:
                    maxPowerAmount += 3;
                    if (PowerManager.GetPowerByKey("MOSSPROPHET", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    if (PowerManager.GetPowerByKey("QUEEN", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    if (PowerManager.GetPowerByKey("GRASSHOPPER", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.KingdomsEdge:
                    maxPowerAmount += 2;
                    if (PowerManager.GetPowerByKey("HIVEQUEEN", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    if (PowerManager.GetPowerByKey("BARDOON", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.Peaks:
                    maxPowerAmount++;
                    if (PowerManager.GetPowerByKey("QUIRREL", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    break;
            }
        }

        foreach (AbstractItem item in Ref.Settings.GetItems())
        {
            string areaName = item.RandoLocation()?.LocationDef?.MapArea ?? "UNKNOWN";
            if (string.Equals(areaName, _randoAreaNames[area]) && (item.name.Contains("Lore_Tablet-") || item.name.Contains("_Inspect")))
            {
                maxPowerAmount++;
                if (item.IsObtained())
                    collectedPowerAmount++;
            }
        }
        // Since neither the fountain nor record bela are randomizeable, we add them manually to the tracker
        if (area == Area.CityOfTears)
        {
            maxPowerAmount += 2;
            if (PowerManager.ObtainedPowers.Any(x => x is TouristPower))
                collectedPowerAmount++;
            if (PowerManager.ObtainedPowers.Any(x => x is OverwhelmingPowerPower))
                collectedPowerAmount++;
        }
        // Same for the dreamer tablet.
        else if (area == Area.RestingGrounds)
        {
            maxPowerAmount++;
            if (PowerManager.ObtainedPowers.Any(x => x is DreamBlessingPower))
                collectedPowerAmount++;
        }

        areaLore = $"{collectedPowerAmount}/{maxPowerAmount}";
        return true;
    }

    #endregion
}
