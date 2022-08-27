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
        {Area.WaterWays, "Royal Waterways"},
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
    }

    internal static void DefineItems()
    {
        if (Settings.RandomizeNpc)
        {
            if (Finder.GetItem("Lore_Tablet-Bretta") == null)
            {
                Finder.DefineCustomItem(NpcItem.CreateItem("Bretta", "BRETTA_DIARY_1", "This is a diary which I found on the street, seems to belong to some girl... No ehm, I didn't read it.", "Bretta"));
                Finder.DefineCustomItem(NpcItem.CreateItem("Elderbug", "ELDERBUG_INTRO_MAIN", "The words of the most powerful being in this world. You should be glad, that it is so cheap.", "Elderbug"));
                Finder.DefineCustomItem(NpcItem.CreateItem("Bardoon", "BIGCAT_TALK_01", "Some caterpillar guy told me this, maybe you can comprehent what this means.", "Bardoon"));
                Finder.DefineCustomItem(NpcItem.CreateItem("Vespa", "HIVEQUEEN_TALK", "A queen from a distant land tortured me with this. Let me share my pain with you.", "Vespa", "Ghosts"));
                Finder.DefineCustomItem(NpcItem.CreateItem("Mask_Maker", "MASK_MAKER_GREET", "From the creator of all those masks, these words shall extend your knowledge of the world.", "Mask_Maker"));
                Finder.DefineCustomItem(NpcItem.CreateItem("Midwife", "SPIDER_MEET", "Please just take this... I don't want this in my shop anymore...", "Midwife"));
                Finder.DefineCustomItem(NpcItem.CreateItem("Gravedigger", "GRAVEDIGGER_TALK", "You might be thinking: Can't I just go outside and talk to him myself? Hey, if people buy water bottles, it was worth a shot.", "Gravedigger", "Ghosts"));
                Finder.DefineCustomItem(NpcItem.CreateItem("Poggy", "POGGY_TALK", "Want to know a secret? I've no clue who Poggy is.", "Poggy", "Ghosts"));
                Finder.DefineCustomItem(NpcItem.CreateItem("Joni", "JONI_TALK", "The last words spoken by the heretic. Should probably cost more than 1000. Buy it now, before I change my mind.", "Joni", "Ghosts"));
                Finder.DefineCustomItem(NpcItem.CreateItem("Myla", "MINER_MEET_1_B", "From the best girl in hallownest. Don't even dare, to hurt her...", "Myla"));
                Finder.DefineCustomItem(NpcItem.CreateItem("Emilitia", "EMILITIA_MEET", "Just take it quick before she wants this back. She will probably pay 1000 times the amount it's worth. Nobody should think, she couldn't afford this, right?", "Emilitia"));
                Finder.DefineCustomItem(NpcItem.CreateItem("Willoh", "GIRAFFE_MEET", "As a bug, I'd be scared of eating unknown mushrooms.", "Willoh"));
                Finder.DefineCustomItem(NpcItem.CreateItem("Moss_Prophet", "MOSS_CULTIST_01", "I'm not sure about this... just take it away.", "Moss_Prophet"));
                Finder.DefineCustomItem(NpcItem.CreateItem("Fluke_Hermit", "FLUKE_HERMIT_IDLE_1", "The words spoken by the child of the A L L U R I N G mother.", "Fluke_Hermit", "CP3"));
                Finder.DefineCustomItem(NpcItem.CreateItem("Quirrel", "QUIRREL_MINES_1", "The wisdom of a real adventurer, which can swim fairly good. :)", "Quirrel"));
                Finder.DefineCustomItem(NpcItem.CreateItem("Queen", "QUEEN_MEET", "The queen sent this through the kingdom for the day, someone like you arrives. How do I know? Well... don't ask.", "Queen"));
                Finder.DefineCustomItem(NpcItem.CreateItem("Marissa", "MARISSA_TALK", "From the most beautiful voice of the world besides me.", "Marissa", "Ghosts"));
                Finder.DefineCustomItem(NpcItem.CreateItem("Grasshopper", "GRASSHOPPER_TALK", "Considering that these live in the garden, being an arsonist might not be the best for them.", "Grasshopper", "Ghosts"));

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

            if(Finder.GetLocation("Gravedigger_Dialogue") == null)
            {
                Finder.DefineCustomLocation(NpcLocation.CreateLocation("Bretta", "Room_Bretta", "Diary"));
                Finder.DefineCustomLocation(NpcLocation.CreateLocation("Elderbug", "Town", "Elderbug"));
                Finder.DefineCustomLocation(NpcLocation.CreateLocation("Bardoon", "Deepnest_East_04", "Big Caterpillar"));
                Finder.DefineCustomLocation(NpcLocation.CreateLocation("Vespa", "Hive_05", "Vespa NPC"));
                Finder.DefineCustomLocation(NpcLocation.CreateLocation("Mask_Maker", "Room_Mask_Maker", "Maskmaker NPC"));
                Finder.DefineCustomLocation(NpcLocation.CreateLocation("Midwife", "Deepnest_41", "NPC"));
                Finder.DefineCustomLocation(NpcLocation.CreateLocation("Gravedigger", "Town", "Gravedigger NPC"));
                Finder.DefineCustomLocation(NpcLocation.CreateLocation("Poggy", "Ruins_Elevator", "Ghost NPC"));
                Finder.DefineCustomLocation(NpcLocation.CreateLocation("Joni", "Cliffs_05", "Ghost NPC Joni"));
                Finder.DefineCustomLocation(NpcLocation.CreateLocation("Myla", "Crossroads_45", "Miner"));
                Finder.DefineCustomLocation(NpcLocation.CreateLocation("Emilitia", "Ruins_House_03", "Emilitia NPC"));
                Finder.DefineCustomLocation(NpcLocation.CreateLocation("Willoh", "Fungus2_34", "Giraffe NPC"));
                Finder.DefineCustomLocation(NpcLocation.CreateLocation("Moss_Prophet", "Fungus3_39", "Moss Cultist"));
                Finder.DefineCustomLocation(NpcLocation.CreateLocation("Fluke_Hermit", "Room_GG_Shortcut", "Fluke Hermit"));
                Finder.DefineCustomLocation(NpcLocation.CreateLocation("Quirrel", "Mines_13", "Quirrel Mines"));
                Finder.DefineCustomLocation(NpcLocation.CreateLocation("Queen", "Room_Queen", "Queen"));
                Finder.DefineCustomLocation(NpcLocation.CreateLocation("Marissa", "Ruins_Bathhouse", "Ghost NPC"));
                Finder.DefineCustomLocation(NpcLocation.CreateLocation("Grasshopper", "Fungus1_24", "Ghost NPC"));
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
            Finder.DefineCustomItem(NpcItem.CreateItem("Xero", "XERO_INSPECT", "The words of the king, for what happens with traitors.", "Xero", "Ghosts"));
            Finder.DefineCustomItem(NpcItem.CreateItem("Elder_Hu", "HU_INSPECT", "The mantis buried him even after he attacked them. How nice of them.", "Elder_Hu", "Ghosts"));
            Finder.DefineCustomItem(NpcItem.CreateItem("Galien", "GALIEN_INSPECT", "Something from an idiot who thought it would be a good idea to an area full a beasts. And then he died... \"Insert Surprised Pikachu Meme here\"", "Galien", "Ghosts"));
            Finder.DefineCustomItem(NpcItem.CreateItem("Marmu", "MUMCAT_INSPECT", "They say the lore master creator hates this damn *****. Whatever that means.", "Marmu", "Ghosts"));
            Finder.DefineCustomItem(NpcItem.CreateItem("Gorb", "ALADAR_INSPECT", "It's gorbin time! (to ascent!) (with Gorb) <br>- Gorb", "Gorb", "Ghosts"));
            Finder.DefineCustomItem(NpcItem.CreateItem("Markoth", "MARKOTH_INSPECT", "Did the shade gate exists before he went in? If so, HOW could've he pass that? Is Markoth void? O.o", "Markoth", "Ghosts"));
            Finder.DefineCustomItem(NpcItem.CreateItem("No_Eyes", "NOEYES_INSPECT", "I wonder what her name was before she... you know. If that's her real name, that is just asking for something to rip out your eyes.", "No_Eyes", "Ghosts"));

            Finder.DefineCustomLocation(TabletLocation.CreateLocation("Xero", "RestingGrounds_02"));
            Finder.DefineCustomLocation(TabletLocation.CreateLocation("Elder_Hu", "Fungus2_32"));
            Finder.DefineCustomLocation(TabletLocation.CreateLocation("Galien", "Deepnest_40"));
            Finder.DefineCustomLocation(TabletLocation.CreateLocation("Marmu", "Fungus3_40"));
            Finder.DefineCustomLocation(TabletLocation.CreateLocation("Gorb", "Cliffs_02"));
            Finder.DefineCustomLocation(TabletLocation.CreateLocation("Markoth", "Deepnest_East_10"));
            Finder.DefineCustomLocation(TabletLocation.CreateLocation("No_Eyes", "Fungus1_35"));
        }

        if (Settings.CursedReading && Finder.GetItem("Reading") == null)
        {
            LoreManager.Instance.CanRead = false;
            Finder.DefineCustomItem(new AbilityItem()
            {
                name = "Reading",
                Item = Enums.CustomItemType.Reading,
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
                moduleName = "Unused" // Same as boolName.
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
                Item = Enums.CustomItemType.Listening,
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

    internal static void ModifyTempleDoor(PlayMakerFSM door)
    {
        if (!RandomizerMod.RandomizerMod.IsRandoSave)
            return;
        if (Settings.BlackEggTempleCondition != RandomizerEndCondition.Dreamers)
            door.GetState("Init").ReplaceAction(new Lambda(() =>
            {
                if ((Settings.BlackEggTempleCondition == RandomizerEndCondition.Lore
                && PowerManager.ActivePowers.Count >= Settings.NeededLore)
                || (Settings.BlackEggTempleCondition == RandomizerEndCondition.DreamersAndLore && door.FsmVariables.FindFsmInt("Guardians Defeated").Value > 2
                && PowerManager.ActivePowers.Count >= Settings.NeededLore))
                    door.SendEvent("READY");
                else
                    door.SendEvent("FINISHED");
            })
            { Name = "End Condition" }, 10);
    }

    /// <summary>
    /// Ensures that joni even spawns, when a "not shiny" item is at the charm location.
    /// </summary>
    /// <param name="joni"></param>
    internal static void ModifyJoni(PlayMakerFSM joni)
    {
        if (!RandomizerMod.RandomizerMod.IsRandoSave)
            return;
        joni.GetState("Idle").ReplaceAction(new Lambda(() =>
        {
            if (PlayerData.instance.GetBool(nameof(PlayerData.instance.hasDreamNail)))
                joni.SendEvent("SHINY PICKED UP");
        }), 0);
    }

    internal static LoreSetOption CheckForRandoFile()
    {
        if (!RandomizerMod.RandomizerMod.IsRandoSave)
        {
            LoreManager.Instance.CanListen = true;
            LoreManager.Instance.CanRead = true;
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

        if(!Settings.RandomizeNpc)
        {
            Power power = null;
            switch (area)
            {
                case Area.Dirtmouth:
                    maxPowerAmount += 2;
                    if (PowerManager.GetPowerByKey("GRAVEDIGGER", out power, false) && PowerManager.ActivePowers.Contains(power))
                        collectedPowerAmount++;
                    if (PowerManager.GetPowerByKey("BRETTA", out power, false) && PowerManager.ActivePowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.Crossroads:
                    maxPowerAmount++;
                    if (PowerManager.GetPowerByKey("MYLA", out power, false) && PowerManager.ActivePowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.Cliffs:
                    maxPowerAmount++;
                    if (PowerManager.GetPowerByKey("JONI", out power, false) && PowerManager.ActivePowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.FungalWastes:
                    maxPowerAmount++;
                    if (PowerManager.GetPowerByKey("WILLOH", out power, false) && PowerManager.ActivePowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.CityOfTears:
                    maxPowerAmount += 3;
                    if (PowerManager.GetPowerByKey("POGGY", out power, false) && PowerManager.ActivePowers.Contains(power))
                        collectedPowerAmount++;
                    if (PowerManager.GetPowerByKey("MARISSA", out power, false) && PowerManager.ActivePowers.Contains(power))
                        collectedPowerAmount++;
                    if (PowerManager.GetPowerByKey("EMILITIA", out power, false) && PowerManager.ActivePowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.WaterWays:
                    maxPowerAmount++;
                    if (PowerManager.GetPowerByKey("FLUKE_HERMIT", out power, false) && PowerManager.ActivePowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.Deepnest:
                    maxPowerAmount += 2;
                    if (PowerManager.GetPowerByKey("MIDWIFE", out power, false) && PowerManager.ActivePowers.Contains(power))
                        collectedPowerAmount++;
                    if (PowerManager.GetPowerByKey("MASKMAKER", out power, false) && PowerManager.ActivePowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.QueensGarden:
                    maxPowerAmount += 3;
                    if (PowerManager.GetPowerByKey("MOSSPROPHET", out power, false) && PowerManager.ActivePowers.Contains(power))
                        collectedPowerAmount++;
                    if (PowerManager.GetPowerByKey("QUEEN", out power, false) && PowerManager.ActivePowers.Contains(power))
                        collectedPowerAmount++;
                    if (PowerManager.GetPowerByKey("GRASSHOPPER", out power, false) && PowerManager.ActivePowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.KingdomsEdge:
                    maxPowerAmount += 2;
                    if (PowerManager.GetPowerByKey("HIVEQUEEN", out power, false) && PowerManager.ActivePowers.Contains(power))
                        collectedPowerAmount++;
                    if (PowerManager.GetPowerByKey("BARDOON", out power, false) && PowerManager.ActivePowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.Peaks:
                    maxPowerAmount++;
                    if (PowerManager.GetPowerByKey("QUIRREL", out power, false) && PowerManager.ActivePowers.Contains(power))
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
        if(area == Area.CityOfTears)
        {
            maxPowerAmount += 2;
            if (PowerManager.ActivePowers.Any(x => x is TouristPower))
                collectedPowerAmount++;
            if (PowerManager.ActivePowers.Any(x => x is OverwhelmingPowerPower))
                collectedPowerAmount++;
        }
        // Same for the dreamer tablet.
        else if(area == Area.RestingGrounds)
        {
            maxPowerAmount++;
            if (PowerManager.ActivePowers.Any(x => x is DreamBlessingPower))
                collectedPowerAmount++;
        }

        areaLore = $"{collectedPowerAmount}/{maxPowerAmount}";
        return true;
    }

    #endregion
}
