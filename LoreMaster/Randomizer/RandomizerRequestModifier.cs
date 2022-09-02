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
            foreach (string name in RandomizerManager.NpcItemNames)
            {
                requestBuilder.AddItemByName("Lore_Tablet-" + name);
                requestBuilder.AddLocationByName(name + "_Dialogue");
                requestBuilder.EditItemRequest("Lore_Tablet-" + name, info =>
                {
                    info.getItemDef = () => new RandomizerMod.RandomizerData.ItemDef()
                    {
                        MajorItem = false,
                        PriceCap = 1,
                        Pool = "Extra Lore",
                        Name = "Lore_Tablet-" + name
                    };
                });
            }
            requestBuilder.AddItemByName("Lore_Page");
            requestBuilder.AddLocationByName("Town_Lore_Page");
        }

        if (RandomizerManager.Settings.RandomizeWarriorStatues)
            foreach (string name in RandomizerManager.StatueItemNames)
            {
                requestBuilder.AddItemByName("Lore_Tablet-" + name);
                requestBuilder.AddLocationByName(name + "_Inspect");
                requestBuilder.EditItemRequest("Lore_Tablet-" + name, info =>
                {
                    info.getItemDef = () => new RandomizerMod.RandomizerData.ItemDef()
                    {
                        MajorItem = false,
                        PriceCap = 1,
                        Pool = "Extra Lore",
                        Name = "Lore_Tablet-" + name
                    };
                });
            }

        if (RandomizerManager.Settings.CursedReading)
        {
            LoreManager.Instance.CanRead = false;
            requestBuilder.AddItemByName("Reading");
            requestBuilder.AddLocationByName("Town_Read");
            requestBuilder.EditItemRequest("Reading", info =>
            {
                info.getItemDef = () => new RandomizerMod.RandomizerData.ItemDef()
                {
                    MajorItem = true,
                    PriceCap = 1250,
                    Name = "Reading",
                    Pool = "Extra Lore"
                };
            });
        }
        else
            LoreManager.Instance.CanRead = true;

        if (RandomizerManager.Settings.CursedListening)
        {
            LoreManager.Instance.CanListen = false;
            requestBuilder.AddItemByName("Listening");
            requestBuilder.AddLocationByName("Town_Listen");
            requestBuilder.EditItemRequest("Listening", info =>
            {
                info.getItemDef = () => new RandomizerMod.RandomizerData.ItemDef()
                {
                    MajorItem = true,
                    PriceCap = 1250,
                    Name = "Listening",
                    Pool = "Extra Lore"
                };
            });
        }
        else
            LoreManager.Instance.CanListen = true;
    }
}