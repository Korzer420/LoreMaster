using UnityEngine;
using System.Reflection;
using ItemChanger.Internal;
using ItemChanger;
using LoreMaster.Helper;
using System;

namespace LoreMaster.CustomItem;

[Serializable]
internal class CustomSprite : ISprite
{
    private string _key = "Lore";

    private bool _isRandoSprite;

    public CustomSprite() { }

    public CustomSprite(string key, bool isRandoSprite = true)
    {
        if (!string.IsNullOrEmpty(key))
            _key = key;
        _isRandoSprite = isRandoSprite;
    }

    [Newtonsoft.Json.JsonIgnore]
    public Sprite Value => SpriteHelper.CreateSprite(_key, _isRandoSprite);

    // public override SpriteManager SpriteManager => new(Assembly.GetExecutingAssembly(), "LoreMaster.Resources.");

    public ISprite Clone() => new CustomSprite(_key,_isRandoSprite);
}
