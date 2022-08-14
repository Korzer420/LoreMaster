using RandomizerMod.RC;

namespace LoreMaster.Randomizer;

internal class RandomizerRequestModifier
{
    public static void Attach()
    {
        RequestBuilder.OnUpdate.Subscribe(-200f, (requestBuilder) => RandomizerManager.DefineItems());
        RequestBuilder.OnUpdate.Subscribe(30f, AddNpcDialogue);
    }

    private static void AddNpcDialogue(RequestBuilder requestBuilder)
    {
        if (RandomizerManager.Settings.RandomizeNpc)
            foreach (string name in RandomizerManager.ItemNames)
            {
                requestBuilder.AddItemByName("Lore_Tablet-" + name);
                requestBuilder.AddLocationByName(name+"_Dialogue");
            }
        if (RandomizerManager.Settings.CursedReading)
        {
            LoreMaster.Instance.CanRead = false;
            requestBuilder.AddItemByName("Reading");
            requestBuilder.AddLocationByName("Read_Town");
        }
        else
            LoreMaster.Instance.CanRead = true;
        if (RandomizerManager.Settings.CursedTalking)
        {
            LoreMaster.Instance.CanListen = false;
            requestBuilder.AddItemByName("Listening");
            requestBuilder.AddLocationByName("Listen_Town");
        }
        else
            LoreMaster.Instance.CanListen = true;
    }
}
