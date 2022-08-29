using ItemChanger;
using LoreMaster.Enums;
using LoreMaster.Helper;
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
    private static Dictionary<string, string> _npcLocationLogic = new()
    {
        {"Bretta", "Room_Bretta[right1] + Rescued_Bretta"},
        {"Elderbug", "Town"},
        {"Bardoon", "Deepnest_East_04[left1] + (WINGS | (LEFTCLAW | RIGHTCLAW) + ENEMYPOGOS) | Deepnest_East_04[right1] + (WINGS | (LEFTCLAW | RIGHTCLAW) + (FIREBALLSKIPS + FIREBALL | LEFTDASH)) | Deepnest_East_04[left2] + (WINGS + RIGHTSUPERDASH + (LEFTCLAW + LEFTDASH | RIGHTDASH) + ACIDSKIPS | ACID) + (WINGS | (LEFTCLAW | RIGHTCLAW) + ENEMYPOGOS)"},
        {"Vespa", "Hive_05[left1] + Defeated_Hive_Knight + DREAMNAIL"},
        {"Mask_Maker", "Room_Mask_Maker[right1]"},
        {"Midwife", "Deepnest_41[left2]"},
        {"Gravedigger", "Town + DREAMNAIL"},
        {"Poggy", "(Ruins_Elevator[left1] | Ruins_Elevator[left2]) + DREAMNAIL"},
        {"Joni", "Cliffs_05[left1] + DREAMNAIL"},
        {"Myla", "Crossroads_45[left1] | Crossroads_45[right1]"},
        {"Emilitia", "Ruins_House_03[left2] + (LEFTCLAW | RIGHTCLAW)"},
        {"Willoh", "Fungus2_34[right1]"},
        {"Moss_Prophet", "Fungus3_39[left1] | Fungus3_39[right1]"},
        {"Fluke_Hermit", "(Room_GG_Shortcut[top1] | (Room_GG_Shortcut[left1] + (LEFTCLAW | RIGHTCLAW))) + SWIM"},
        {"Quirrel", "(Mines_13[bot1] + (LEFTCLAW | RIGHTCLAW)) | Mines_13[top1] | Mines_13[right1]"},
        {"Grasshopper", "Fungus1_24[left1] + (LEFTCLAW | RIGHTCLAW | WINGS) + DREAMNAIL" },
        {"Marissa", "(Ruins_Bathhouse[door1] | Ruins_Bathhouse[right1]) + DREAMNAIL" },
        {"Queen", "Room_Queen[left1]"}
    };

    private static Dictionary<string, string> _warriorStatueLocationLogic = new()
    {
        {"Xero", "((RestingGrounds_02[left1] | RestingGrounds_02[right1] | RestingGrounds_02[bot1]) + (ENEMYPOGOS | RIGHTCLAW | LEFTCLAW | WINGS | RIGHTSUPERDASH)) | RestingGrounds_02[top1]" },
        {"Elder_Hu", "Fungus2_32[left1]" },
        {"Galien", "Deepnest_40[right1]" },
        {"Marmu", "Fungus3_40[top1] | ((Can_Stag + Queen's_Gardens_Stag) | Fungus3_40[right1]) + (LEFTDASH | LEFTCLAW | WINGS)" },
        {"Gorb", "((Cliffs_02[left2] | Cliffs_02[door1]) + (RIGHTCLAW | ENEMYPOGOS | WINGS | RIGHTDASH)) | Cliffs_02[left1] | Cliffs_02[bot1] + RIGHTCLAW" },
        {"Markoth", "Deepnest_East_10[left1]" },
        {"No_Eyes", "(Fungus1_35[left1] | Fungus1_35[right1]) + (DARKROOMS | LANTERN)" }
    };

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
        RCData.RuntimeLogicOverride.Subscribe(60f, ModifyConnectionLogic);
    }

    private static void ModifyLogic(GenerationSettings settings, LogicManagerBuilder builder)
    {
        // If the condition is changed, we need to add that first in order for the npc items to be considered.
        if (RandomizerManager.Settings.BlackEggTempleCondition != RandomizerEndCondition.Dreamers)
        {
            Term loreAmount = builder.GetOrAddTerm("LORE");
            builder.DoLogicEdit(new("Opened_Black_Egg_Temple",
                "Room_temple[left1] + LORE>" + (RandomizerManager.Settings.NeededLore - 1)
                + (RandomizerManager.Settings.BlackEggTempleCondition == RandomizerEndCondition.DreamersAndLore ? "+ DREAMER>2" : null)));
            foreach (string key in RandomizerHelper.TabletNames.Keys)
                builder.AddItem(new SingleItem("Lore_Tablet-" + key, new(loreAmount, 1)));
        }

        if (RandomizerManager.Settings.RandomizeNpc)
        {
            foreach (string key in _npcLocationLogic.Keys)
            {
                builder.AddLogicDef(new(key + "_Dialogue", RandomizerManager.Settings.CursedListening ? $"({_npcLocationLogic[key]}) + LISTEN" : _npcLocationLogic[key]));
                if (RandomizerManager.Settings.BlackEggTempleCondition == RandomizerEndCondition.Dreamers)
                    builder.AddItem(new EmptyItem("Lore_Tablet-" + key));
                else
                    builder.AddItem(new SingleItem("Lore_Tablet-" + key, new(builder.GetTerm("LORE"), 1)));
            }
            builder.AddLogicDef(new("Town_Lore_Page", "Town"));
            builder.AddItem(new EmptyItem("Lore_Page"));
        }

        if (RandomizerManager.Settings.RandomizeWarriorStatues)
            foreach (string key in _warriorStatueLocationLogic.Keys)
            {
                builder.AddLogicDef(new(key + "_Inspect", RandomizerManager.Settings.CursedReading ? $"({_warriorStatueLocationLogic[key]}) + READ" : _warriorStatueLocationLogic[key]));

                if (RandomizerManager.Settings.BlackEggTempleCondition == RandomizerEndCondition.Dreamers)
                    builder.AddItem(new EmptyItem("Lore_Tablet-" + key));
                else
                    builder.AddItem(new SingleItem("Lore_Tablet-" + key, new(builder.GetTerm("LORE"), 1)));
            }

        if (RandomizerManager.Settings.CursedReading)
        {
            Term readAbility = builder.GetOrAddTerm("READ");
            builder.AddLogicDef(new("Town_Read", "Town"));
            builder.AddItem(new BoolItem("Reading", readAbility));
            using Stream stream = typeof(LogicManager).Assembly.GetManifestResourceStream("LoreMaster.Resources.Randomizer.ReadLogicModifier.json");
            builder.DeserializeJson(LogicManagerBuilder.JsonType.LogicEdit, stream);
        }

        if (RandomizerManager.Settings.CursedListening)
        {
            Term listenAbility = builder.GetOrAddTerm("LISTEN");
            builder.AddLogicDef(new("Town_Listen", "Town"));
            builder.AddItem(new BoolItem("Listening", listenAbility));
            using Stream stream = typeof(LogicManager).Assembly.GetManifestResourceStream("LoreMaster.Resources.Randomizer.ListenLogicModifier.json");
            builder.DeserializeJson(LogicManagerBuilder.JsonType.LogicEdit, stream);
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
            if (Finder.GetLocation($"Journal_Entry-Void_Idol_1") != null)
                builder.DoLogicEdit(new($"Journal_Entry-Void_Idol_1", "(ORIG) + READ"));
        }
        // Extra logic for rando plus.
        if (RandomizerManager.Settings.CursedListening)
            for (int i = 0; i < _randoPlusLocation.Count; i++)
                if (Finder.GetLocation(_randoPlusLocation[i]) != null)
                    builder.DoLogicEdit(new(_randoPlusLocation[i], "(ORIG) + LISTEN"));
    }
}
