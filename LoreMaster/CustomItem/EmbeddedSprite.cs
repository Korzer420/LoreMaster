using UnityEngine;
using System.Reflection;
using ItemChanger.Internal;
using ItemChanger;
using LoreMaster.Helper;
using System;

namespace LoreMaster.CustomItem;

[Serializable]
internal class EmbeddedSprite : ISprite
{
    private string _key = "Lore";

    private static SpriteManager _manager = new(Assembly.GetExecutingAssembly(), "LoreMaster.Resources.");

    public EmbeddedSprite()
    {

    }

    public EmbeddedSprite(string key)
    {
        if (!string.IsNullOrEmpty(key))
            _key = key;
    }

    [Newtonsoft.Json.JsonIgnore]
    public Sprite Value => SpriteHelper.CreateSprite(_key, !string.Equals(_key, "Lore"));

    public ISprite Clone() => new EmbeddedSprite(_key);
}
