using LoreMaster.ItemChangerData.Other;
using System.Collections.Generic;
using static ItemChanger.ItemNames;
using static LoreMaster.ItemChangerData.Other.ItemList;

namespace LoreMaster.Helper;

internal static class RandomizerHelper
{
    public static Dictionary<string, string> TabletNames { get; } = new()
    {
         {Lore_Tablet_City_Entrance,"RUINS_TAB_01"},
         {Lore_Tablet_Pleasure_House,"RUINS_MARISSA_POSTER"},
         {Lore_Tablet_Sanctum_Entrance,"MAGE_COMP_03"},
         {ItemList.Lore_Tablet_Record_Bela, "MAGE_COMP_02" },
         {Lore_Tablet_Sanctum_Past_Soul_Master,"MAGE_COMP_01"},
         {Lore_Tablet_Watchers_Spire,"LURIAN_JOURNAL"},
         {Lore_Tablet_Archives_Upper,"ARCHIVE_01"},
         {Lore_Tablet_Archives_Left,"ARCHIVE_03"},
         {Lore_Tablet_Archives_Right,"ARCHIVE_02"},
         {Lore_Tablet_Pilgrims_Way_1,"PILGRIM_TAB_01"},
         {Lore_Tablet_Pilgrims_Way_2,"PILGRIM_TAB_02"},
         {Lore_Tablet_Mantis_Outskirts,"MANTIS_PLAQUE_01"},
         {Lore_Tablet_Mantis_Village,"MANTIS_PLAQUE_02"},
         {Lore_Tablet_Greenpath_Upper_Hidden,"GREEN_TABLET_06"},
         {Lore_Tablet_Greenpath_Below_Toll,"GREEN_TABLET_01"},
         {Lore_Tablet_Greenpath_Lifeblood,"GREEN_TABLET_03"},
         {Lore_Tablet_Greenpath_Stag,"GREEN_TABLET_05"},
         {Lore_Tablet_Greenpath_QG,"GREEN_TABLET_07"},
         {Lore_Tablet_Greenpath_Lower_Hidden,"GREEN_TABLET_02"},
         {Lore_Tablet_Dung_Defender,"DUNG_DEF_SIGN"},
         {Lore_Tablet_Spore_Shroom,"FUNG_TAB_04"},
         {Lore_Tablet_Fungal_Wastes_Hidden,"FUNG_TAB_03"},
         {Lore_Tablet_Fungal_Wastes_Below_Shrumal_Ogres,"FUNG_TAB_01"},
         {Lore_Tablet_Fungal_Core,"FUNG_TAB_02"},
         {Lore_Tablet_Ancient_Basin,"ABYSS_TUT_TAB_01"},
         {Lore_Tablet_Kings_Pass_Focus,"TUT_TAB_01"},
         {Lore_Tablet_Kings_Pass_Fury,"TUT_TAB_03"},
         {Lore_Tablet_Kings_Pass_Exit,"TUT_TAB_02"},
         {Lore_Tablet_World_Sense,"COMPLETION_RATE_UNLOCKED"},
         {Lore_Tablet_Howling_Cliffs,"CLIFF_TAB_02"},
         {Lore_Tablet_Kingdoms_Edge,"MR_MUSH_RIDDLE_TAB_NORMAL"},
         {Lore_Tablet_Palace_Workshop,"WP_WORKSHOP_01"},
         {Lore_Tablet_Palace_Throne,"WP_THRONE_01"},
         {Lore_Tablet_Path_of_Pain_Entrance,"PLAQUE_WARN"},
         {Path_of_Pain_Reward, "POP" },
         {Lemm_Sign, "RELICDEALER_DOOR" },
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
         {Stag_Egg_Inspect, "STAG_EGG_INSPECT"}
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
