using Modding;
using RandomizerMod.RC;

namespace LoreMaster.ModInterop;

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
	}

	private static void CheckForLoreRando(RequestBuilder builder)
	{

	}

    #endregion

    #region Eventhandler

    private static void ApplySettings(RequestBuilder builder)
	{
		if (!LoreMaster.Instance.RandomizerSettings.Enabled)
			return;
	    if (builder.gs.PoolSettings.LoreTablets)
		{
			foreach (string loreTablet in LoreCore.Data.ItemList.LoreTablets)
			{
				
			}
		}
		else
		{

		}
	}

    #endregion
}
