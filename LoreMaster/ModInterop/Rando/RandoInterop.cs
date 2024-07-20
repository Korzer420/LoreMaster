using ItemChanger;
using Modding;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerMod.Logging;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using System.Collections.Generic;
using System.Linq;

namespace LoreMaster.ModInterop.Rando;

internal static class RandoInterop
{
    #region Properties

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

    internal static void Initialize()
    {
        RequestBuilder.OnUpdate.Subscribe(31f, ApplySettings);
        RCData.RuntimeLogicOverride.Subscribe(10000f, ModifyLogic);
        RandoController.OnCalculateHash += RandoController_OnCalculateHash;
        SettingsLog.AfterLogSettings += SettingsLog_AfterLogSettings;
    }

    private static void SettingsLog_AfterLogSettings(LogArguments log, System.IO.TextWriter textWriter)
    {
        textWriter.WriteLine("LoreMaster settings");
        using Newtonsoft.Json.JsonTextWriter jsonTextWriter = new(textWriter) { CloseOutput = false };

        RandomizerMod.RandomizerData.JsonUtil._js.Serialize(jsonTextWriter, LoreMaster.Instance.RandomizerSettings);
        textWriter.WriteLine();
    }

    private static int RandoController_OnCalculateHash(RandoController controller, int hash)
     => !LoreMaster.Instance.RandomizerSettings.Enabled 
            ? 0 
            : 21;
    
    private static void ModifyLogic(GenerationSettings settings, LogicManagerBuilder builder)
    {
        if (!LoreMaster.Instance.RandomizerSettings.Enabled)
            return;

        if (LoreMaster.Instance.RandomizerSettings.RandomizeElderbugRewards)
        {
            builder.AddItem(new EmptyItem("Small_Glyph"));
            builder.AddItem(new EmptyItem("Minor_Glyph"));
            builder.AddItem(new EmptyItem("Major_Glyph"));
            builder.AddItem(new EmptyItem("Cleansing_Scroll"));
            builder.AddItem(new EmptyItem("Mystical_Scroll"));
        }

        bool includeLoreRando = builder.Terms.Any(x => x.Name == "LORE");
        foreach (string loreTablet in LoreCore.Data.ItemList.LoreTablets)
            if (includeLoreRando)
                builder.AddItem(new SingleItem($"{loreTablet}_Empowered", new(builder.GetTerm("LORE"), 1)));
            else
                builder.AddItem(new EmptyItem($"{loreTablet}_Empowered"));
    }

    #endregion

    #region Eventhandler

    private static void ApplySettings(RequestBuilder builder)
    {
        if (!LoreMaster.Instance.RandomizerSettings.Enabled)
            return;

        if (LoreMaster.Instance.RandomizerSettings.RandomizeElderbugRewards)
        {
            builder.AddItemByName("Small_Glyph", 8);
            builder.AddItemByName("Minor_Glyph", 5);
            builder.AddItemByName("Major_Glyph", 3);
            builder.AddItemByName("Cleansing_Scroll");
            builder.AddItemByName("Mystical_Scroll");

            builder.EditItemRequest("Small_Glyph", info =>
            {
                info.getItemDef = () => new()
                {
                    Name = "Small_Glyph",
                    Pool = "Lore",
                    PriceCap = 250,
                    MajorItem = false
                };
            });
            builder.EditItemRequest("Minor_Glyph", info =>
            {
                info.getItemDef = () => new()
                {
                    Name = "Minor_Glyph",
                    Pool = "Lore",
                    PriceCap = 500,
                    MajorItem = false
                };
            });
            builder.EditItemRequest("Major_Glyph", info =>
            {
                info.getItemDef = () => new()
                {
                    Name = "Major_Glyph",
                    Pool = "Lore",
                    PriceCap = 750,
                    MajorItem = false
                };
            });
            builder.EditItemRequest("Cleansing_Scroll", info =>
            {
                info.getItemDef = () => new()
                {
                    Name = "Cleansing_Scroll",
                    Pool = "Lore",
                    PriceCap = 400,
                    MajorItem = false
                };
            });
            builder.EditItemRequest("Mystical_Scroll", info =>
            {
                info.getItemDef = () => new()
                {
                    Name = "Mystical_Scroll",
                    Pool = "Lore",
                    PriceCap = 400,
                    MajorItem = false
                };
            });
        }

        if (builder.gs.PoolSettings.LoreTablets)
        {
            IEnumerable<string> allItems = builder.EnumerateItemGroups()
            .SelectMany(x => x.Items.EnumerateDistinct())
            .ToList();

            foreach (string loreTablet in LoreCore.Data.ItemList.LoreTablets)
            {
                if (allItems.Contains(loreTablet))
                    builder.RemoveItemByName(loreTablet);
                // LoreRando already added this, so we can skip it.
                else if (allItems.Contains($"{loreTablet}_Empowered"))
                    continue;
                builder.AddItemByName($"{loreTablet}_Empowered");
                builder.EditItemRequest($"{loreTablet}_Empowered", info =>
                {
                    info.getItemDef = () => new()
                    {
                        Name = $"{loreTablet}_Empowered",
                        PriceCap = 1,
                        Pool = "Lore",
                        MajorItem = false
                    };
                });
            }
        }
        else
        {
            // The focus and world sense tablets are unreadable if focus and world sense are randomized, but lore tablets aren't. We just at them to the start in this case.
            if (builder.gs.NoveltySettings.RandomizeFocus && builder.StartItems.GetCount(ItemNames.Lore_Tablet_Kings_Pass_Focus + "_Empowered") == 0)
                builder.AddToStart(ItemNames.Lore_Tablet_Kings_Pass_Focus + "_Empowered");
            if (builder.gs.PoolSettings.Dreamers && builder.StartItems.GetCount(ItemNames.Lore_Tablet_World_Sense + "_Empowered") == 0)
                builder.AddToStart(ItemNames.Lore_Tablet_World_Sense + "_Empowered");
        }
    }

    #endregion
}
