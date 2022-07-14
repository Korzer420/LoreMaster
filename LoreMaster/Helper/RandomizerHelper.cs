using System.Collections.Generic;

namespace LoreMaster.Helper;

internal static class RandomizerHelper
{
    private static readonly Dictionary<string, string> _tabletNames = new()
    {
         {"City_Entrance","RUINS_TAB_01"},
         {"Pleasure_House","RUINS_MARISSA_POSTER"},
         {"Sanctum_Entrance","MAGE_COMP_03"},
         {"Sanctum_Past_Soul_Master","MAGE_COMP_01"},
         {"Watcher's_Spire","LURIAN_JOURNAL"},
         {"Archives_Upper","ARCHIVE_01"},
         {"Archives_Left","ARCHIVE_03"},
         {"Archives_Right","ARCHIVE_02"},
         {"Pilgrim's_Way_1","PILGRIM_TAB_01"},
         {"Pilgrim's_Way_2","PILGRIM_TAB_02"},
         {"Mantis_Outskirts","MANTIS_PLAQUE_01"},
         {"Mantis_Village","MANTIS_PLAQUE_02"},
         {"Greenpath_Upper_Hidden","GREEN_TABLET_06"},
         {"Greenpath_Below_Toll","GREEN_TABLET_01"},
         {"Greenpath_Lifeblood","GREEN_TABLET_03"},
         {"Greenpath_Stag","GREEN_TABLET_05"},
         {"Greenpath_QG","GREEN_TABLET_07"},
         {"Greenpath_Lower_Hidden","GREEN_TABLET_02"},
         {"Dung_Defender","DUNG_DEF_SIGN"},
         {"Spore_Shroom","FUNG_TAB_04"},
         {"Fungal_Wastes_Hidden","FUNG_TAB_03"},
         {"Fungal_Wastes_Below_Shrumal_Ogres","FUNG_TAB_01"},
         {"Fungal_Core","FUNG_TAB_02"},
         {"Ancient_Basin","ABYSS_TUT_TAB_01"},
         {"King's_Pass_Focus","TUT_TAB_01"},
         {"King's_Pass_Fury","TUT_TAB_03"},
         {"King's_Pass_Exit","TUT_TAB_02"},
         {"World_Sense","COMPLETION_RATE_UNLOCKED"},
         {"Howling_Cliffs","CLIFF_TAB_02"},
         {"Kingdom's_Edge","MR_MUSH_RIDDLE_TAB_NORMAL"},
         {"Palace_Workshop","WP_WORKSHOP_01"},
         {"Palace_Throne","WP_THRONE_01"},
         {"Path_of_Pain_Entrance","PLAQUE_WARN"},
    };

    /// <summary>
    /// Translates the name that the rando uses to the official ingame key.
    /// </summary>
    /// <param name="tabletName"></param>
    /// <returns></returns>
    internal static string TranslateRandoName(string tabletName)
    {
        if (_tabletNames.ContainsKey(tabletName))
            return _tabletNames[tabletName];
        else
        {
            LoreMaster.Instance.LogError("Unrecognizable lore tablet name: "+tabletName+". Please report this to the mod maker.");
            return null;
        }
    }
}
