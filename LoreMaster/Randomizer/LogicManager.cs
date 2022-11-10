using ItemChanger;
using ItemChanger.Extensions;
using LoreMaster.Enums;
using LoreMaster.ItemChangerData.Locations;
using LoreMaster.ItemChangerData.Other;
using LoreMaster.Manager;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LoreMaster.Randomizer;

public class LogicManager
{
    private static List<string> _randoPlusLocation = new()
    {
        "Mr_Mushroom-Fungal_Wastes",
        "Mr_Mushroom-Kingdom's_Edge",
        "Mr_Mushroom-Deepnest",
        "Mr_Mushroom-Howling_Cliffs",
        "Mr_Mushroom-Ancient_Basin",
        "Mr_Mushroom-Fog_Canyon",
        "Mr_Mushroom-King's_Pass",
        "Nailsmith_Upgrade_1",
        "Nailsmith_Upgrade_2",
        "Nailsmith_Upgrade_3",
        "Nailsmith_Upgrade_4"
    };

    public static void AttachLogic()
    {
        RCData.RuntimeLogicOverride.Subscribe(20f, ModifyLogic);
        RCData.RuntimeLogicOverride.Subscribe(600f, ModifyConnectionLogic);
    }

    private static void ModifyLogic(GenerationSettings settings, LogicManagerBuilder builder)
    {
        Term loreTerm = null;
        if (RandomizerManager.Settings.BlackEggTempleCondition != BlackEggTempleCondition.Dreamers
            || RandomizerManager.Settings.RandomizeElderbugRewards
            || RandomizerManager.Settings.DefineRefs)
        {
            loreTerm = builder.GetOrAddTerm("LORE");
            foreach (Area area in RandomizerRequestModifier.LoreItems.Keys)
                foreach (string item in RandomizerRequestModifier.LoreItems[area])
                    builder.AddItem(new SingleItem(item + "_Empowered", new(loreTerm, 1)));

            if (RandomizerManager.Settings.BlackEggTempleCondition != BlackEggTempleCondition.Dreamers)
                builder.DoLogicEdit(new("Opened_Black_Egg_Temple", "Room_temple[left1] + "
                    + (RandomizerManager.Settings.BlackEggTempleCondition == BlackEggTempleCondition.Lore
                    ? "LORE>" + RandomizerManager.Settings.NeededLore
                    : "DREAMER>2 + " + "LORE>" + RandomizerManager.Settings.NeededLore)));
        }
        else
            foreach (Area area in RandomizerRequestModifier.LoreItems.Keys)
                foreach (string item in RandomizerRequestModifier.LoreItems[area])
                    builder.AddItem(new EmptyItem(item + "_Empowered"));

        if (RandomizerManager.Settings.CursedListening)
        {
            Term listenTerm = builder.GetOrAddTerm("LISTEN");
            builder.AddItem(new BoolItem(ItemList.Listen_Ability, listenTerm));
        }

        if (RandomizerManager.Settings.CursedReading)
        {
            Term readTerm = builder.GetOrAddTerm("READ");
            builder.AddItem(new BoolItem(ItemList.Read_Ability, readTerm));
        }

        if (RandomizerManager.Settings.RandomizeNpc || RandomizerManager.Settings.DefineRefs)
        {
            using Stream stream = typeof(LogicManager).Assembly.GetManifestResourceStream("LoreMaster.Resources.Randomizer.Logic.NpcLogic.json");
            builder.DeserializeJson(LogicManagerBuilder.JsonType.Locations, stream);
            if (RandomizerManager.Settings.CursedListening)
                foreach (string npc in RandomizerRequestModifier.NpcLocations)
                    builder.DoLogicEdit(new(npc, "(ORIG) + LISTEN"));

            // Let npc dialogue count towards lore conditions.
            if (loreTerm != null)
                foreach (string item in RandomizerRequestModifier.NpcItems)
                    builder.AddItem(new SingleItem(item, new(loreTerm, 1)));
            else
                foreach (string item in RandomizerRequestModifier.NpcItems)
                    builder.AddItem(new EmptyItem(item));
        }

        if (RandomizerManager.Settings.RandomizeDreamWarriorStatues || RandomizerManager.Settings.DefineRefs)
        {
            using Stream stream = typeof(LogicManager).Assembly.GetManifestResourceStream("LoreMaster.Resources.Randomizer.Logic.DreamWarriorLogic.json");
            builder.DeserializeJson(LogicManagerBuilder.JsonType.Locations, stream);
            if (RandomizerManager.Settings.CursedReading)
                foreach (string warrior in RandomizerRequestModifier.DreamWarriorLocations)
                    builder.DoLogicEdit(new(warrior, "(ORIG) + READ"));

            // Let dream warrior inspects count towards lore conditions.
            if (loreTerm != null)
                foreach (string item in RandomizerRequestModifier.DreamWarriorItems)
                    builder.AddItem(new SingleItem(item, new(loreTerm, 1)));
            else
                foreach (string item in RandomizerRequestModifier.DreamWarriorItems)
                    builder.AddItem(new EmptyItem(item));
        }

        if (RandomizerManager.Settings.RandomizeDreamDialogue || RandomizerManager.Settings.DefineRefs)
        {
            using Stream stream = typeof(LogicManager).Assembly.GetManifestResourceStream("LoreMaster.Resources.Randomizer.Logic.DreamLogic.json");
            builder.DeserializeJson(LogicManagerBuilder.JsonType.Locations, stream);

            // The crystal shaman and Pale King items cannot be obtained until their dream dialogue has been acquired.
            builder.DoLogicEdit(new(LocationNames.Descending_Dark, "(ORIG) + DREAMNAIL"));
            builder.DoLogicEdit(new(LocationNames.King_Fragment, "(ORIG) + DREAMNAIL"));

            // Let dream dialogue count towards lore conditions.
            if (loreTerm != null)
                foreach (string item in RandomizerRequestModifier.DreamItems)
                    builder.AddItem(new SingleItem(item, new(loreTerm, 1)));
            else
                foreach (string item in RandomizerRequestModifier.DreamItems)
                    builder.AddItem(new EmptyItem(item));
        }

        if (RandomizerManager.Settings.RandomizePointsOfInterest || RandomizerManager.Settings.DefineRefs)
        {
            using Stream stream = typeof(LogicManager).Assembly.GetManifestResourceStream("LoreMaster.Resources.Randomizer.Logic.PointOfInterestLogic.json");
            builder.DeserializeJson(LogicManagerBuilder.JsonType.Locations, stream);

            if (RandomizerManager.Settings.CursedReading)
            {
                builder.DoLogicEdit(new(LocationList.Dreamer_Tablet, "(ORIG) + READ"));
                builder.DoLogicEdit(new(LocationList.City_Fountain, "(ORIG) + READ"));
                builder.DoLogicEdit(new(LocationList.Lore_Tablet_Record_Bela, "(ORIG) + READ"));
                builder.DoLogicEdit(new(LocationList.Traitor_Grave, "(ORIG) + READ"));
            }

            // Let point of interest count towards lore conditions.
            if (loreTerm != null)
                foreach (string item in RandomizerRequestModifier.PointOfInterestItems)
                    builder.AddItem(new SingleItem(item, new(loreTerm, 1)));
            else
                foreach (string item in RandomizerRequestModifier.PointOfInterestItems)
                    builder.AddItem(new EmptyItem(item));
        }
        else
            builder.AddLogicDef(new("Stag_Nest", "(Cliffs_03[right1] | Stag_Nest_Stag | STAGS > 8) + ((LEFTCLAW | WINGS) + LEFTDASH | LEFTSUPERDASH)"));
        builder.AddItem(new EmptyItem(ItemList.Stag_Egg));

        if (RandomizerManager.Settings.RandomizeTravellers || RandomizerManager.Settings.DefineRefs)
        {
            try
            {
                // Define terms
                builder.GetOrAddTerm("QUIRREL");
                builder.GetOrAddTerm("CLOTH");
                builder.GetOrAddTerm("TISO");
                builder.GetOrAddTerm("ZOTE");
                using Stream stream = typeof(LogicManager).Assembly.GetManifestResourceStream("LoreMaster.Resources.Randomizer.Logic.TravellerLogic.json");
                builder.DeserializeJson(LogicManagerBuilder.JsonType.Locations, stream);

                // Determine the spawn order.
                LoreManager.Instance.Traveller.Clear();
                // Force the same generation for the same seed.
                LoreMaster.Instance.Generator = new System.Random(settings.Seed);
                LoreManager.Instance.Traveller.Add(Traveller.Quirrel, new()
                {
                    CurrentStage = RandomizerManager.Settings.TravellerOrder == TravelOrder.Everywhere ? 10 : 0,
                    Locations = TravellerLocation.DetermineOrder(Traveller.Quirrel, RandomizerManager.Settings.TravellerOrder == TravelOrder.Shuffled
                    && RandomizerManager.Settings.RandomizeTravellers).ToArray()
                });
                LoreManager.Instance.Traveller.Add(Traveller.Cloth, new()
                {
                    CurrentStage = RandomizerManager.Settings.TravellerOrder == TravelOrder.Everywhere ? 10 : 0,
                    Locations = TravellerLocation.DetermineOrder(Traveller.Cloth, RandomizerManager.Settings.TravellerOrder == TravelOrder.Shuffled
                    && RandomizerManager.Settings.RandomizeTravellers).ToArray()
                });
                LoreManager.Instance.Traveller.Add(Traveller.Zote, new()
                {
                    CurrentStage = RandomizerManager.Settings.TravellerOrder == TravelOrder.Everywhere ? 10 : 0,
                    Locations = TravellerLocation.DetermineOrder(Traveller.Zote, RandomizerManager.Settings.TravellerOrder == TravelOrder.Shuffled
                    && RandomizerManager.Settings.RandomizeTravellers).ToArray()
                });
                LoreManager.Instance.Traveller.Add(Traveller.Tiso, new()
                {
                    CurrentStage = RandomizerManager.Settings.TravellerOrder == TravelOrder.Everywhere ? 10 : 0,
                    Locations = TravellerLocation.DetermineOrder(Traveller.Tiso, RandomizerManager.Settings.TravellerOrder == TravelOrder.Shuffled
                    && RandomizerManager.Settings.RandomizeTravellers).ToArray()
                });
                // Modify the logic for the order in which the traveller stages appear.
                Term currentTerm = builder.GetOrAddTerm("QUIRREL");
                Traveller currentTraveller = Traveller.Quirrel;
                for (int i = 0; i < RandomizerRequestModifier.TravellerItems.Length - 1; i++)
                {
                    if (i == 9 || i == 14 || i == 19)
                    {
                        currentTraveller++;
                        currentTerm = builder.GetOrAddTerm(currentTraveller.ToString().ToUpper());
                        continue;
                    }
                    if (Finder.GetLocation(RandomizerRequestModifier.TravellerLocations[i]) is not TravellerLocation travellerLocation)
                        continue;

                    int stage = LoreManager.Instance.Traveller[currentTraveller].Locations.IndexOf(travellerLocation.ObjectName);
                    if (stage != 0)
                        builder.DoLogicEdit(new(RandomizerRequestModifier.TravellerLocations[i], RandomizerManager.Settings.CursedListening
                        ? $"(ORIG) + LISTEN + {currentTerm.ToString().ToUpper()}>" + (stage - 1)
                        : $"(ORIG) + {currentTraveller.ToString().ToUpper()}>" + (stage - 1)));
                    else if (RandomizerManager.Settings.CursedListening)
                        builder.DoLogicEdit(new(RandomizerRequestModifier.TravellerLocations[i], "(ORIG) + LISTEN"));

                    if (loreTerm != null)
                        builder.AddItem(new MultiItem(RandomizerRequestModifier.TravellerItems[i], new RandomizerCore.TermValue[] { new(currentTerm, 1), new(loreTerm, 1) }));
                    else
                        builder.AddItem(new SingleItem(RandomizerRequestModifier.TravellerItems[i], new(currentTerm, 1)));
                }

                // Modify special locations.
                if (RandomizerManager.Settings.CursedListening)
                {
                    builder.DoLogicEdit(new(LocationList.Quirrel_Blue_Lake, "(ORIG) + LISTEN"));
                    builder.DoLogicEdit(new(LocationList.Cloth_End, "(ORIG) + LISTEN"));
                    builder.DoLogicEdit(new(LocationList.Zote_Dirtmouth_After_Colosseum, "(ORIG) + LISTEN"));

                    // Because Zote sucks...
                    int stage = LoreManager.Instance.Traveller[Traveller.Zote].Locations.IndexOf("Zote Buzzer Convo(Clone)");
                    if (stage == 0)
                        builder.DoLogicEdit(new(LocationList.Zote_Greenpath, "(ORIG) + LISTEN"));
                    else
                        builder.DoLogicEdit(new(LocationList.Zote_Greenpath, "(ORIG) + LISTEN + ZOTE>" + (stage - 1)));
                    stage = LoreManager.Instance.Traveller[Traveller.Zote].Locations.IndexOf("/Zote Deepnest/Faller/NPC");
                    if (stage == 0)
                        builder.DoLogicEdit(new(LocationList.Zote_Deepnest, "(ORIG) + LISTEN"));
                    else
                        builder.DoLogicEdit(new(LocationList.Zote_Deepnest, "(ORIG) + LISTEN + ZOTE>" + (stage - 1)));
                }
                else
                {
                    // Because Zote sucks...
                    int stage = LoreManager.Instance.Traveller[Traveller.Zote].Locations.IndexOf("Zote Buzzer Convo(Clone)");
                    if (stage != 0)
                        builder.DoLogicEdit(new(LocationList.Zote_Greenpath, "(ORIG) + ZOTE>" + (stage - 1)));
                    stage = LoreManager.Instance.Traveller[Traveller.Zote].Locations.IndexOf("/Zote Deepnest/Faller/NPC");
                    if (stage != 0)
                        builder.DoLogicEdit(new(LocationList.Zote_Deepnest, "(ORIG) + ZOTE>" + (stage - 1)));
                }

                // Add "special" items.
                if (loreTerm != null)
                {
                    builder.AddItem(new MultiItem(ItemList.Dialogue_Quirrel_Blue_Lake, new RandomizerCore.TermValue[]
                    {
                        new(builder.GetOrAddTerm("QUIRREL"), 1),
                        new(loreTerm, 1)
                    }));
                    builder.AddItem(new MultiItem(ItemList.Dialogue_Cloth_Ghost, new RandomizerCore.TermValue[]
                    {
                        new(builder.GetOrAddTerm("CLOTH"), 1),
                        new(loreTerm, 1)
                    }));
                    builder.AddItem(new MultiItem(ItemList.Dream_Dialogue_Tiso_Corpse, new RandomizerCore.TermValue[]
                    {
                        new(builder.GetOrAddTerm("TISO"), 1),
                        new(loreTerm, 1)
                    }));
                    builder.AddItem(new MultiItem(ItemList.Dialogue_Zote_Dirtmouth_After_Colosseum, new RandomizerCore.TermValue[]
                    {
                        new(builder.GetOrAddTerm("ZOTE"), 1),
                        new(loreTerm, 1)
                    }));
                    builder.AddItem(new MultiItem(ItemList.Dialogue_Zote_Greenpath, new RandomizerCore.TermValue[]
                    {
                        new(builder.GetOrAddTerm("ZOTE"), 1),
                        new(loreTerm, 1)
                    }));
                    builder.AddItem(new MultiItem(ItemList.Dialogue_Zote_Deepnest, new RandomizerCore.TermValue[]
                    {
                        new(builder.GetOrAddTerm("ZOTE"), 1),
                        new(loreTerm, 1)
                    }));
                }
                else
                {
                    builder.AddItem(new SingleItem(ItemList.Dialogue_Quirrel_Blue_Lake, new(builder.GetOrAddTerm("QUIRREL"), 1)));
                    builder.AddItem(new SingleItem(ItemList.Dialogue_Cloth_Ghost, new(builder.GetOrAddTerm("CLOTH"), 1)));
                    builder.AddItem(new SingleItem(ItemList.Dream_Dialogue_Tiso_Corpse, new(builder.GetOrAddTerm("TISO"), 1)));
                    builder.AddItem(new SingleItem(ItemList.Dialogue_Zote_Dirtmouth_After_Colosseum, new(builder.GetOrAddTerm("ZOTE"), 1)));
                    builder.AddItem(new SingleItem(ItemList.Dialogue_Zote_Greenpath, new(builder.GetOrAddTerm("ZOTE"), 1)));
                    builder.AddItem(new SingleItem(ItemList.Dialogue_Zote_Deepnest, new(builder.GetOrAddTerm("ZOTE"), 1)));
                }
            }
            catch (System.Exception exception)
            {
                LoreMaster.Instance.LogError(exception.Message);
                LoreMaster.Instance.LogError(exception.StackTrace);
            }
        }

        if (RandomizerManager.Settings.RandomizeTreasures || RandomizerManager.Settings.DefineRefs)
        {
            Term iselda = builder.GetOrAddTerm("CHARTSAVAILABLE");
            builder.AddItem(new BoolItem(ItemList.Lemm_Order, iselda));
            using Stream treasureStream = typeof(LogicManager).Assembly.GetManifestResourceStream("LoreMaster.Resources.Randomizer.Logic.TreasureLogic.json");
            builder.DeserializeJson(LogicManagerBuilder.JsonType.Locations, treasureStream);
            if (RandomizerManager.Settings.CursedListening)
                for (int i = 0; i < 14; i++)
                    builder.DoLogicEdit(new(RandomizerRequestModifier.TreasureLocation[i], "(ORIG) + LISTEN"));

            builder.AddItem(new MultiItem(ItemList.Magical_Key, new RandomizerCore.TermValue[]
            {
                new(builder.GetTerm("SIMPLE"), 4),
                new(builder.GetTerm("ELEGANT"), 1),
                new(builder.GetTerm("Love_Key"), 1),
                new(builder.GetTerm("King's_Brand"), 1)
            }));
            foreach (string item in RandomizerRequestModifier.TreasureItems.Skip(1))
                builder.AddItem(new EmptyItem(item));
            if (RandomizerManager.Settings.CursedReading)
                builder.DoLogicEdit(new(LocationList.Lemm_Door, "(ORIG) + READ"));

            if (RandomizerManager.Settings.ForceCompassForTreasure)
            {
                Term compass = builder.GetOrAddTerm("COMPASS");
                builder.AddItem(new BoolItem(ItemNames.Wayward_Compass, compass));
                foreach (string location in RandomizerRequestModifier.TreasureLocation)
                    builder.DoLogicEdit(new(location, "(ORIG) + COMPASS"));
            }
            if (RandomizerManager.Settings.RandomizeTreasures)
                builder.AddItem(new EmptyItem(ItemList.Lemm_Sign));
        }

        if (RandomizerManager.Settings.CursedListening)
        {
            using Stream listenStream = typeof(LogicManager).Assembly.GetManifestResourceStream("LoreMaster.Resources.Randomizer.Logic.ListenLogicModifier.json");
            builder.DeserializeJson(LogicManagerBuilder.JsonType.LogicEdit, listenStream);
        }

        if (RandomizerManager.Settings.CursedReading)
        {
            using Stream readStream = typeof(LogicManager).Assembly.GetManifestResourceStream("LoreMaster.Resources.Randomizer.Logic.ReadLogicModifier.json");
            builder.DeserializeJson(LogicManagerBuilder.JsonType.LogicEdit, readStream);
        }

        if (RandomizerManager.Settings.RandomizeElderbugRewards || RandomizerManager.Settings.DefineRefs)
        {
            using Stream stream = typeof(LogicManager).Assembly.GetManifestResourceStream("LoreMaster.Resources.Randomizer.Logic.ElderbugLogic.json");
            builder.DeserializeJson(LogicManagerBuilder.JsonType.Locations, stream);
            builder.AddItem(new EmptyItem(ItemList.Joker_Scroll));
            builder.AddItem(new EmptyItem(ItemList.Cleansing_Scroll));
            builder.AddItem(new EmptyItem(ItemList.Cleansing_Scroll_Double));
        }
    }

    private static void ModifyConnectionLogic(GenerationSettings settings, LogicManagerBuilder builder)
    {
        // Extra logic for journal rando
        if (RandomizerManager.Settings.CursedReading)
        {
            if (builder.Waypoints.Contains("Defeated_Colosseum_3"))
                builder.DoLogicEdit(new("Defeated_Colosseum_3", "(ORIG) + READ"));
            // Since level 2 and 3 from the void idle takes the logic from level 1. We only need to modify one.
            if (Finder.GetLocation("Journal_Entry-Void_Idol_1") != null && builder.LogicLookup.Any(x => string.Equals("Journal_Entry-Void_Idol_1", x.Key, System.StringComparison.CurrentCultureIgnoreCase)))
                builder.DoLogicEdit(new("Journal_Entry-Void_Idol_1", "(ORIG) + READ"));
        }
        // Extra logic for rando plus.
        if (RandomizerManager.Settings.CursedListening)
        {
            for (int i = 0; i < _randoPlusLocation.Count; i++)
                if (Finder.GetLocation(_randoPlusLocation[i]) != null && builder.LogicLookup.Any(x => string.Equals(_randoPlusLocation[i], x.Key, System.StringComparison.CurrentCultureIgnoreCase)))
                    builder.DoLogicEdit(new(_randoPlusLocation[i], "(ORIG) + LISTEN"));
            if (builder.LogicLookup.Any(x => x.Key == "Hunter's_Notes-Nightmare_King" || x.Key == "Journal_Entry-Nightmare_King"))
                builder.DoLogicEdit(new("Defeated_Any_Nightmare_King", "(ORIG) + LISTEN"));
        }

        if (builder.IsTerm("WALLET") && (RandomizerManager.Settings.RandomizeTreasures || RandomizerManager.Settings.DefineRefs))
            for (int i = 6; i < 14; i++)
                if (i < 10)
                    builder.DoLogicEdit(new(RandomizerRequestModifier.TreasureLocation[i], "(ORIG) + WALLET"));
                else if (i < 13)
                    builder.DoLogicEdit(new(RandomizerRequestModifier.TreasureLocation[i], "(ORIG) + WALLET>1"));
                else
                    builder.DoLogicEdit(new(RandomizerRequestModifier.TreasureLocation[i], "(ORIG) + WALLET>2"));
    }
}
