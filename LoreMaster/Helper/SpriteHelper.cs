using System.IO;
using UnityEngine;

namespace LoreMaster.Helper;

public static class SpriteHelper
{
    public static Sprite CreateSprite(string spriteName)
    {
        string imageFile = Path.Combine(Path.GetDirectoryName(typeof(LoreMaster).Assembly.Location), "Resources/"+spriteName+".png");
        byte[] imageData = File.ReadAllBytes(imageFile);
        Texture2D tex = new(1, 1, TextureFormat.RGBA32, false);
        ImageConversion.LoadImage(tex, imageData, true);
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
    }
}
