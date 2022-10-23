using System.Collections.Generic;
using static ItemChanger.ItemNames;
using static LoreMaster.ItemChangerData.Other.ItemList;

namespace LoreMaster.Helper;

internal static class RandomizerHelper
{
    public static Dictionary<string, string> TabletNames { get; } = new()
    {
         {Lore_Tablet_City_Entrance+"_Empowered","RUIN_TAB_01"},
         {Lore_Tablet_Pleasure_House+"_Empowered","RUINS_MARISSA_POSTER"},
         {Lore_Tablet_Sanctum_Entrance+"_Empowered","MAGE_COMP_03"},
         {Lore_Tablet_Record_Bela, "MAGE_COMP_02" },
         {Lore_Tablet_Sanctum_Past_Soul_Master+"_Empowered","MAGE_COMP_01"},
         {Lore_Tablet_Watchers_Spire+"_Empowered","LURIAN_JOURNAL"},
         {Lore_Tablet_Archives_Upper+"_Empowered","ARCHIVE_01"},
         {Lore_Tablet_Archives_Left+"_Empowered","ARCHIVE_03"},
         {Lore_Tablet_Archives_Right+"_Empowered","ARCHIVE_02"},
         {Lore_Tablet_Pilgrims_Way_1+"_Empowered","PILGRIM_TAB_01"},
         {Lore_Tablet_Pilgrims_Way_2+"_Empowered","PILGRIM_TAB_02"},
         {Lore_Tablet_Mantis_Outskirts+"_Empowered","MANTIS_PLAQUE_01"},
         {Lore_Tablet_Mantis_Village+"_Empowered","MANTIS_PLAQUE_02"},
         {Lore_Tablet_Greenpath_Upper_Hidden+"_Empowered","GREEN_TABLET_06"},
         {Lore_Tablet_Greenpath_Below_Toll+"_Empowered","GREEN_TABLET_01"},
         {Lore_Tablet_Greenpath_Lifeblood+"_Empowered","GREEN_TABLET_03"},
         {Lore_Tablet_Greenpath_Stag+"_Empowered","GREEN_TABLET_05"},
         {Lore_Tablet_Greenpath_QG+"_Empowered","GREEN_TABLET_07"},
         {Lore_Tablet_Greenpath_Lower_Hidden+"_Empowered","GREEN_TABLET_02"},
         {Lore_Tablet_Dung_Defender+"_Empowered","DUNG_DEF_SIGN"},
         {Lore_Tablet_Spore_Shroom+"_Empowered","FUNG_TAB_04"},
         {Lore_Tablet_Fungal_Wastes_Hidden+"_Empowered","FUNG_TAB_03"},
         {Lore_Tablet_Fungal_Wastes_Below_Shrumal_Ogres+"_Empowered","FUNG_TAB_01"},
         {Lore_Tablet_Fungal_Core+"_Empowered","FUNG_TAB_02"},
         {Lore_Tablet_Ancient_Basin+"_Empowered","ABYSS_TUT_TAB_01"},
         {Lore_Tablet_Kings_Pass_Focus+"_Empowered","TUT_TAB_01"},
         {Lore_Tablet_Kings_Pass_Fury+"_Empowered","TUT_TAB_03"},
         {Lore_Tablet_Kings_Pass_Exit+"_Empowered","TUT_TAB_02"},
         {Lore_Tablet_World_Sense+"_Empowered","COMPLETION_RATE_UNLOCKED"},
         {Lore_Tablet_Howling_Cliffs+"_Empowered","CLIFF_TAB_02"},
         {Lore_Tablet_Kingdoms_Edge+"_Empowered","MR_MUSH_RIDDLE_TAB_NORMAL"},
         {Lore_Tablet_Palace_Workshop+"_Empowered","WP_WORKSHOP_01"},
         {Lore_Tablet_Palace_Throne+"_Empowered","WP_THRONE_01"},
         {Lore_Tablet_Path_of_Pain_Entrance+"_Empowered","PLAQUE_WARN"},
         {Path_of_Pain_Reward, "POP" },
         {Lemm_Sign, "RELICDEALER_DOOR" },
         { Dialogue_Menderbug_Diary, "MENDERBUG" },
         { Traitor_Grave, "XUN_GRAVE_INSPECT" },
         {Dialogue_Gravedigger, "GRAVEDIGGER" },
         {Dialogue_Poggy, "POGGY" },
         {Dialogue_Emilitia, "EMILITIA" },
         {Dialogue_Quirrel_Peaks, "QUIRREL" },
         {Dialogue_Joni, "JONI" },
         {Dialogue_Vespa, "HIVEQUEEN" },
         {Dialogue_Bardoon, "BARDOON" },
         {Dialogue_Moss_Prophet, "MOSSPROPHET" },
         {Dialogue_Queen, "QUEEN" },
         {Dialogue_Myla, "MYLA" },
         {Dialogue_Fluke_Hermit, "FLUKE_HERMIT" },
         {Dialogue_Bretta_Diary, "BRETTA" },
         {Dialogue_Midwife, "MIDWIFE" },
         {Dialogue_Mask_Maker, "MASKMAKER" },
         {Dialogue_Willoh, "WILLOH" },
         {Dialogue_Grasshopper, "GRASSHOPPER" },
         {Dialogue_Marissa, "MARISSA" },
         {Dialogue_Tiso_Crossroads, "TISO" },
         {Dialogue_Zote_Deepnest, "ZOTE" },
         {Stag_Egg_Inspect, "STAG_EGG_INSPECT"},
         {Inscription_City_Fountain, "FOUNTAIN_PLAQUE_DESC" },
         {Inscription_Dreamer_Tablet, "DREAMERS_INSPECT_RG5" }
    };

    /// <summary>
    /// Translates the name that the rando uses to the official ingame key.
    /// </summary>
    /// <param name="tabletName"></param>
    /// <returns></returns>
    internal static string TranslateRandoName(string tabletName)
    {
        if (TabletNames.ContainsKey(tabletName))
            return TabletNames[tabletName];
        else
            return null;
    }
}
