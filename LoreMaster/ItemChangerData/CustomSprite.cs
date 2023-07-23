using ItemChanger;
using KorzUtils.Helper;
using System;
using UnityEngine;

namespace LoreMaster.ItemChangerData;

[Serializable]
public class WrappedSprite : ISprite
{
    #region Constructors

    public WrappedSprite() { }

    public WrappedSprite(string key)
    {
        if (!string.IsNullOrEmpty(key))
            Key = key;
    }

    #endregion

    #region Properties

    public string Key { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    public Sprite Value => SpriteHelper.CreateSprite<LoreMaster>("Sprites." + Key.Replace("/", ".").Replace("\\", "."));

    #endregion

    public ISprite Clone() => new WrappedSprite(Key);
}
