using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.UIDefs;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using LoreMaster.Randomizer.Items;
using System.Collections.Generic;

namespace LoreMaster.Randomizer;

public static class RandomizerManager
{
    #region Members

    private static RandomizerSettings _settings;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the settings of a randomizer.
    /// </summary>
    public static RandomizerSettings Settings => _settings ??= new();

    public static List<string> ItemNames => new()
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
        "Queen"
    };

    #endregion

    #region Methods

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
        if (Settings.RandomizeNpc && Finder.GetItem("Lore_Tablet-Bretta") == null)
        {
            Finder.DefineCustomItem(NpcItem.CreateItem("Bretta", "BRETTA_DIARY_1", "This is a diary which I found on the street, seems to belong to some girl... No ehm, I didn't read it.", "Randomizer/Bretta"));
            Finder.DefineCustomItem(NpcItem.CreateItem("Elderbug", "ELDERBUG_INTRO_MAIN", "The words of the most powerful being in this world. You should be glad, that it is so cheap.", "Randomizer/Elderbug"));
            Finder.DefineCustomItem(NpcItem.CreateItem("Bardoon", "BIGCAT_TALK_01", "Some caterpillar guy told me this, maybe you can comprehent what this means.", "Randomizer/Bardoon"));
            Finder.DefineCustomItem(NpcItem.CreateItem("Vespa", "HIVEQUEEN_TALK", "A queen from a distant land tortured me with this. Let me share my pain with you.", "Randomizer/Vespa", "Ghosts"));
            Finder.DefineCustomItem(NpcItem.CreateItem("Mask_Maker", "MASK_MAKER_GREET", "From the creator of all those masks, these words shall extend your knowledge of the world.", "Randomizer/Mask_Maker"));
            Finder.DefineCustomItem(NpcItem.CreateItem("Midwife", "SPIDER_MEET", "Please just take this... I don't want this in my shop anymore...", "Randomizer/Midwife"));
            Finder.DefineCustomItem(NpcItem.CreateItem("Gravedigger", "GRAVEDIGGER_TALK", "You might be thinking: Can't I just go outside and talk to him myself? Hey, if people buy water bottles, it was worth a shot.", "Randomizer/Gravedigger", "Ghosts"));
            Finder.DefineCustomItem(NpcItem.CreateItem("Poggy", "POGGY_TALK", "Want to know a secret? I've no clue who Poggy is.", "Randomizer/Poggy", "Ghosts"));
            Finder.DefineCustomItem(NpcItem.CreateItem("Joni", "JONI_TALK", "The last words spoken by the heretic. Should probably cost more than 1000. Buy it now, before I change my mind.", "Randomizer/Joni", "Ghosts"));
            Finder.DefineCustomItem(NpcItem.CreateItem("Myla", "MINER_MEET_1_B", "From the best girl in hallownest. Don't even dare, to hurt her...", "Randomizer/Myla"));
            Finder.DefineCustomItem(NpcItem.CreateItem("Emilitia", "EMILITIA_MEET", "Just take it quick before she wants this back. She will probably pay 1000 times the amount it's worth. Nobody should think, she couldn't afford this, right?", "Randomizer/Emilitia"));
            Finder.DefineCustomItem(NpcItem.CreateItem("Willoh", "GIRAFFE_MEET", "As a bug, I'd be scared of eating unknown mushrooms.", "Randomizer/Willoh"));
            Finder.DefineCustomItem(NpcItem.CreateItem("Moss_Prophet", "MOSS_CULTIST_01", "I'm not sure about this... just take it away.", "Randomizer/Moss_Prophet"));
            Finder.DefineCustomItem(NpcItem.CreateItem("Fluke_Hermit", "FLUKE_HERMIT_IDLE_1", "The words spoken by the child of the A L L U R I N G mother.", "Randomizer/Fluke_Hermit", "CP3"));
            Finder.DefineCustomItem(NpcItem.CreateItem("Quirrel", "QUIRREL_MINES_1", "The wisdom of a real adventurer, which can swim fairly good. :)", "Randomizer/Quirrel"));
            Finder.DefineCustomItem(NpcItem.CreateItem("Queen", "QUEEN_MEET", "The queen sent this through the kingdom for the day, someone like you arrives. How do I know? Well... don't ask.", "Randomizer/Queen"));
            
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
            Finder.DefineCustomLocation(new AbilityLocation() 
            {
                forceShiny = true,
                sceneName = "Town",
                name = "Town_Lore_Page",
                x = 107.61f,
                y = 11.41f
            });
        }

        if (Settings.CursedReading && Finder.GetItem("Reading") == null)
        {
            LoreMaster.Instance.CanRead = false;
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
            LoreMaster.Instance.CanListen = false;
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
        if (Settings.BlackEggTempleCondition != RandomizerEndCondition.Dreamers)
            door.GetState("Init").ReplaceAction(new Lambda(() =>
            {
                if ((Settings.BlackEggTempleCondition == RandomizerEndCondition.Lore
                && LoreMaster.Instance.ActivePowers.Count >= Settings.NeededLore)
                || (Settings.BlackEggTempleCondition == RandomizerEndCondition.DreamersAndLore && door.FsmVariables.FindFsmInt("Guardians Defeated").Value > 2
                && LoreMaster.Instance.ActivePowers.Count >= Settings.NeededLore))
                    door.SendEvent("READY");
                else
                    door.SendEvent("FINISHED");
            })
            { Name = "End Condition" }, 10);
    }

    internal static void CheckForRandoFile() 
    { 
        if (!RandomizerMod.RandomizerMod.IsRandoSave)
        {
            LoreMaster.Instance.Log("Give ability");
            LoreMaster.Instance.CanListen = true;
            LoreMaster.Instance.CanRead = true;
        }
    }

    #endregion
}
