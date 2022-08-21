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

    private bool _isRandoSprite;

    private static SpriteManager _manager = new(Assembly.GetExecutingAssembly(), "LoreMaster.Resources.");

    public EmbeddedSprite() { }

    public EmbeddedSprite(string key, bool isRandoSprite = true)
    {
        if (!string.IsNullOrEmpty(key))
            _key = key;
        _isRandoSprite = isRandoSprite;
    }

    [Newtonsoft.Json.JsonIgnore]
    public Sprite Value => SpriteHelper.CreateSprite(_key, _isRandoSprite);

    public ISprite Clone() => new EmbeddedSprite(_key,_isRandoSprite);
}
