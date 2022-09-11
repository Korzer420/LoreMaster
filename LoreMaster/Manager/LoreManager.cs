using LoreMaster.Enums;
using LoreMaster.LorePowers;
using LoreMaster.LorePowers.RestingGrounds;
using System.Collections.Generic;

namespace LoreMaster.Manager;

/// <summary>
/// Manager for handling the lore related logic.
/// </summary>
internal class LoreManager
{
    #region Constructors

    public LoreManager() => Instance = this;
    
    #endregion

    #region Properties

    public List<Power> ActivePowers { get; set; } = new();

    public bool UseCustomText { get; set; } = true;

    public bool UseHints { get; set; } = true;

    /// <summary>
    /// Gets or sets the value, that indicates if the player can read lore tablets. (Rando only)
    /// </summary>
    public bool CanRead { get; set; } = true;

    /// <summary>
    /// Gets or sets the value, that indicates if the player can listen to npc.
    /// </summary>
    public bool CanListen { get; set; } = true;

    public static LoreManager Instance { get; set; }

    #endregion

    #region Event handler

    /// <summary>
    /// This is the main control, which determines which power is on.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="sheetTitle"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    internal string GetText(string key, string sheetTitle, string text)
    {
        if (key.Equals("LoreMaster"))
            return "Lore Powers";
        key = ModifyKey(key);
        if (key.Equals("INV_NAME_SUPERDASH"))
        {
            bool hasDiamondDash = PowerManager.HasObtainedPower("MYLA");
            bool hasDiamondCore = PowerManager.HasObtainedPower("QUIRREL");

            if (hasDiamondDash && !hasDiamondCore)
                text = Properties.AdditionalText.CORELESS_DIAMOND_HEART_NAME;
            else if (!hasDiamondDash && hasDiamondCore)
                text = Properties.AdditionalText.SHELLLESS_DIAMOND_HEART_NAME;
            else if (hasDiamondCore && hasDiamondDash)
                text = Properties.AdditionalText.FULL_DIAMOND_HEART_NAME;
        }
        else if (key.Equals("INV_DESC_SUPERDASH"))
        {
            bool hasDiamondDash = PowerManager.HasObtainedPower("MYLA");
            bool hasDiamondCore = PowerManager.HasObtainedPower("QUIRREL");

            if (hasDiamondDash && !hasDiamondCore)
                text += Properties.AdditionalText.CORELESS_DIAMOND_HEART_DESCRIPTION;
            else if (!hasDiamondDash && hasDiamondCore)
                text += Properties.AdditionalText.SHELLLESS_DIAMOND_HEART_DESCRIPTION;
            else if (hasDiamondCore && hasDiamondDash)
                text += Properties.AdditionalText.FULL_DIAMOND_HEART_DESCRIPTION;
        }
        else if (key.Equals("FOUNTAIN_PLAQUE_DESC"))
        {
            PowerManager.GetPowerByKey(key, out Power fountain, false);
            text += " [" + fountain.PowerName + "] " + (UseHints ? fountain.Hint : fountain.Description);
        }
        else if (key.Contains("DREAMERS_INSPECT_RG"))
        {
            PowerManager.GetPowerByKey("DREAMERS_INSPECT_RG5", out Power dreamer, false);
            text += ((DreamBlessingPower)dreamer).GetExtraText(key);
        }
        else if (!ModifyText(key, ref text) && PowerManager.HasObtainedPower("QUEEN"))
        {
            if (key.Equals("CHARM_NAME_12"))
                return "Queen's Thorns";
            else if (key.Equals("CHARM_DESC_12"))
                return text + "<br>Blessed by the white lady, which causes them to drain soul and sometimes energy from their victims. Leash out more agile.";
        }
        else if (key.Equals("ELDERBUG_INTRO_MAIN"))
            text = Properties.AdditionalText.ELDERBUG_INTRO_MAIN;
            //text = "This town may not hold the most interesting wisdom, but the kingdom below sure does. There a plenty of tablets and creatures which you can learn from. " +
            //    "Maybe someday, I'll be able to call you \"LoreMaster\". Oh what a thought, excuse me. Anyway, if you want to explore the world below, keeping track of " +
            //    "every knowledge that you acquired might be hard. Let me help you with that. This is a relic which tracks every bit of information that you've collected so far. " +
            //    "Sometimes, the knowledge can be more of a threat than a blessing. In those cases, touching the ability on the relic while resting may disable them, until you touch it again. " +
            //    "Maybe you should not waste too much time though. I heard legends that this artifact might lock it's " +
            //    "power behind a test or something once the one in the time event \"Patch 1.3\" happens... whatever that might be. Don't forget \"Knowledge is power\".";
        return text;
    }

    #endregion

    #region Methods

    public bool ModifyText(string key, ref string displayText)
    {
        try
        {
            if (PowerManager.GetPowerByKey(key, out Power power))
            {
                if (power.Tag != PowerTag.Remove)
                {
                    if (LoreManager.Instance.UseCustomText && !string.IsNullOrEmpty(power.CustomText))
                        displayText = power.CustomText;
                    displayText += "<br>[" + power.PowerName + "]";
                    displayText += "<br>" + (UseHints ? power.Hint : power.Description);
                }
                if (string.Equals(key, "PLAQUE_WARN"))
                {
                    PowerManager.GetPowerByKey("POP", out Power popPower, false);
                    if (popPower.Tag != PowerTag.Remove)
                    {
                        displayText += "<page>For those, that reveals the secret, awaits the power:";
                        displayText += "<br>[" + popPower.PowerName + "] ";
                        displayText += "<br>" + (UseHints ? popPower.Hint : popPower.Description);
                    }
                }
                return true;
            }
        }
        catch (System.Exception exception)
        {
            LoreMaster.Instance.LogError("An error occured while modifying the text: " + exception.Message);
            LoreMaster.Instance.LogError(exception.StackTrace);
        }
        return false;
    }

    /// <summary>
    /// Modifies the language key, to keep consistancy between the lore keys (mostly for NPC).
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private string ModifyKey(string key)
    {
        if (key.Contains("BRETTA_DIARY"))
            key = "BRETTA";
        else if (string.Equals(key,"HIVEQUEEN_TALK") || string.Equals(key,"HIVEQUEEN_REPEAT"))
            key = "HIVEQUEEN";
        else if (string.Equals(key,"JONI_TALK") || string.Equals(key,"JONI_REPEAT"))
            key = "JONI";
        else if (string.Equals(key,"POGGY_TALK") || string.Equals(key,"POGGY_REPEAT"))
            key = "POGGY";
        else if (string.Equals(key,"GRAVEDIGGER_TALK") || string.Equals(key,"GRAVEDIGGER_REPEAT"))
            key = "GRAVEDIGGER";
        else if (string.Equals(key,"GRASSHOPPER_TALK") || string.Equals(key,"GRASSHOPPER_REPEAT"))
            key = "GRASSHOPPER";
        else if (string.Equals(key, "MARISSA_TALK") || string.Equals(key, "MARISSA_REPEAT"))
            key = "MARISSA";
        else if (IsMidwife(key))
            key = "MIDWIFE";
        else if (IsBardoon(key))
            key = "BARDOON";
        else if (IsFlukeHermit(key))
            key = "FLUKE_HERMIT";
        else if (IsQueen(key))
            key = "QUEEN";
        else if (IsMaskMaker(key))
            key = "MASKMAKER";
        else if (IsWilloh(key))
            key = "WILLOH";
        else if (IsMyla(key))
            key = "MYLA";
        else if (IsQuirrel(key))
            key = "QUIRREL";
        else if (IsEmilitia(key))
            key = "EMILITIA";
        else if (IsMossProphet(key))
            key = "MOSSPROPHET";
        return key;
    }

    #region NPC Dialogues

    private bool IsBardoon(string key)
    {
        return string.Equals(key,"BIGCAT_INTRO") || string.Equals(key,"BIGCAT_TALK_01")
            || string.Equals(key,"BIGCAT_TALK_02") || string.Equals(key,"BIGCAT_TALK_03")
            || string.Equals(key,"BIGCAT_TAIL_HIT") || string.Equals(key,"BIGCAT_KING_BRAND")
            || string.Equals(key,"BIGCAT_SHADECHARM") || string.Equals(key,"BIGCAT_REPEAT");
    }

    private bool IsMidwife(string key)
    {
        return string.Equals(key,"SPIDER_MEET") || string.Equals(key,"SPIDER_GREET")
            || string.Equals(key,"SPIDER_GREET2") || string.Equals(key,"SPIDER_REPEAT") || string.Equals(key,"MIDWIFE_WEAVERSONG");
    }

    private bool IsMaskMaker(string key)
    {
        return string.Equals(key,"MASK_MAKER_GREET") || string.Equals(key,"MASK_MAKER_REPEAT")
            || string.Equals(key,"MASK_MAKER_REPEAT2") || string.Equals(key,"MASK_MAKER_REPEAT3")
            || string.Equals(key,"MASK_MAKER_UNMASK") || string.Equals(key,"MASK_MAKER_UNMASK3")
            || string.Equals(key,"MASK_MAKER_UNMASK4") || string.Equals(key,"MASK_MAKER_UNMASK2") || string.Equals(key,"MASK_MAKER_UNMASK_REPEAT")
            || string.Equals(key,"MASKMAKER_GREET") || string.Equals(key,"MASKMAKER_REPEAT")
            || string.Equals(key,"MASKMAKER_REPEAT2") || string.Equals(key,"MASKMAKER_REPEAT3")
            || string.Equals(key,"MASKMAKER_UNMASK") || string.Equals(key,"MASKMAKER_UNMASK3")
            || string.Equals(key,"MASKMAKER_UNMASK4") || string.Equals(key,"MASKMAKER_UNMASK2") || string.Equals(key,"MASKMAKER_UNMASK_REPEAT");
    }

    private bool IsFlukeHermit(string key)
    {
        return string.Equals(key,"FLUKE_HERMIT_PRAY") || string.Equals(key,"FLUKE_HERMIT_PRAY_REPEAT")
            || string.Equals(key,"FLUKE_HERMIT_IDLE_1") || string.Equals(key,"FLUKE_HERMIT_IDLE_2")
            || string.Equals(key,"FLUKE_HERMIT_IDLE_3") || string.Equals(key,"FLUKE_HERMIT_IDLE_4")
            || string.Equals(key,"FLUKE_HERMIT_IDLE_5") || string.Equals(key,"MASK_MAKER_UNMASK2") || string.Equals(key,"MASK_MAKER_UNMASK_REPEAT");
    }

    private bool IsQueen(string key)
    {
        return string.Equals(key,"QUEEN_MEET") || string.Equals(key,"QUEEN_MEET_REPEAT")
            || string.Equals(key,"QUEEN_TALK_01") || string.Equals(key,"QUEEN_TALK_02")
            || string.Equals(key,"QUEEN_HORNET") || string.Equals(key,"QUEEN_DUNG")
            || string.Equals(key,"QUEEN_DUNG_02") || string.Equals(key,"QUEEN_REPEAT_KINGSOUL")
            || string.Equals(key,"QUEEN_TALK_EXTRA") || string.Equals(key,"QUEEN_REPEAT_SHADECHARM")
            || string.Equals(key,"QUEEN_GRIMMCHILD") || string.Equals(key," QUEEN_GRIMMCHILD_FULL");
    }

    private bool IsWilloh(string key)
    {
        return string.Equals(key,"GIRAFFE_MEET") || string.Equals(key,"GIRAFFE_LOWER") || string.Equals(key,"GIRAFFE_LOWER_REPEAT");
    }

    private bool IsMyla(string key)
    {
        return string.Equals(key,"MINER_MEET_1_B") || string.Equals(key,"MINER_MEET_REPEAT") || string.Equals(key,"MINER_EARLY_1_B") || string.Equals(key,"MINER_EARLY_2_B") || string.Equals(key,"MINER_EARLY_3");
    }

    private bool IsQuirrel(string key)
    {
        return string.Equals(key,"QUIRREL_MINES_1") || string.Equals(key,"QUIRREL_MINES_2") || string.Equals(key,"QUIRREL_MINES_3") || string.Equals(key,"QUIRREL_MINES_4");
    }

    private bool IsEmilitia(string key)
    {
        return string.Equals(key,"EMILITIA_MEET") || string.Equals(key,"EMILITIA_KING_BRAND") || string.Equals(key,"EMILITIA_GREET") || string.Equals(key,"EMILITIA_REPEAT");
    }

    private bool IsMossProphet(string key)
    {
        return string.Equals(key,"MOSS_CULTIST_01") || string.Equals(key,"MOSS_CULTIST_02") || string.Equals(key,"MOSS_CULTIST_03");
    }

    #endregion

    #endregion
}
