using ItemChanger;
using LoreMaster.Helper;
using System;
using UnityEngine;

namespace LoreMaster.CustomItem;

[Serializable]
internal class CustomSprite : ISprite
{
    public CustomSprite() { }

    public CustomSprite(string key, bool isRandoSprite = true)
    {
        if (!string.IsNullOrEmpty(key))
            Key = key;
        RandoSprite = isRandoSprite;
    }

    public string Key { get; set; } = "Lore";

    public bool RandoSprite { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    public Sprite Value => SpriteHelper.CreateSprite(Key, RandoSprite);

    public ISprite Clone() => new CustomSprite(Key, RandoSprite);
}
