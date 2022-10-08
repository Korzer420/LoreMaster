using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LoreMaster.Helper;

public static class SpriteHelper
{
    private static Dictionary<string, Sprite> _cachedSprites = new();

    public static Sprite CreateSprite(string spriteName, bool randoResource = false) => CreateSprite(spriteName, ".png", randoResource);
    
    /// <summary>
    /// Creates a sprite from the given image path. Starts in this Resource folder.
    /// <para/> Eg. using "Lore" is sprite name will look for LoreMaster\Resources\Base\Lore. (If <paramref name="randoResource"/> is false)
    /// </summary>
    public static Sprite CreateSprite(string spriteName, string extension, bool randoResource = false)
    {
        if (!_cachedSprites.ContainsKey(spriteName))
        {
            string imageFile = Path.Combine(Path.GetDirectoryName(typeof(LoreMaster).Assembly.Location), "Resources\\" + (randoResource ? "Randomizer\\" : "Base\\") + spriteName + extension);
            byte[] imageData = File.ReadAllBytes(imageFile);
            Texture2D tex = new(1, 1, TextureFormat.RGBA32, false);
            ImageConversion.LoadImage(tex, imageData, true);
            tex.filterMode = FilterMode.Bilinear;
            _cachedSprites.Add(spriteName, Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f)));
        }
        return _cachedSprites[spriteName];
    }
}
