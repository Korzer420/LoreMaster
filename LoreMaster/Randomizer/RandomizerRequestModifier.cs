using LoreMaster.Manager;
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
            LoreManager.Instance.CanRead = false;
            requestBuilder.AddItemByName("Reading");
            requestBuilder.AddLocationByName("Town_Read");
        }
        else
            LoreManager.Instance.CanRead = true;
        if (RandomizerManager.Settings.CursedListening)
        {
            LoreManager.Instance.CanListen = false;
            requestBuilder.AddItemByName("Listening");
            requestBuilder.AddLocationByName("Town_Listen");
        }
        else
            LoreManager.Instance.CanListen = true;
    }
}
