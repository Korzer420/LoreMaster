using RandomizerMod.RC;

namespace LoreMaster.Randomizer;

internal class RandomizerRequestModifier
{
    public static void ModifyRequest()
    {
        RequestBuilder.OnUpdate.Subscribe(-200f, (requestBuilder) => RandomizerManager.DefineItems());
        RequestBuilder.OnUpdate.Subscribe(30f, AddNpcDialogue);
    }

    private static void AddNpcDialogue(RequestBuilder requestBuilder)
    {
        if (RandomizerManager.Settings.RandomizeNpc)
        {
            foreach (string name in RandomizerManager.ItemNames)
            {
                requestBuilder.AddItemByName("Lore_Tablet-" + name);
                requestBuilder.AddLocationByName(name + "_Dialogue");
            }
            requestBuilder.AddItemByName("Lore_Page");
            requestBuilder.AddLocationByName("Town_Lore_Page");
        }
        if (RandomizerManager.Settings.CursedReading)
        {
            LoreMaster.Instance.CanRead = false;
            requestBuilder.AddItemByName("Reading");
            requestBuilder.AddLocationByName("Town_Read");
        }
        else
            LoreMaster.Instance.CanRead = true;
        if (RandomizerManager.Settings.CursedListening)
        {
            LoreMaster.Instance.CanListen = false;
            requestBuilder.AddItemByName("Listening");
            requestBuilder.AddLocationByName("Town_Listen");
        }
        else
            LoreMaster.Instance.CanListen = true;
    }
}
