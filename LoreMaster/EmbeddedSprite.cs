using UnityEngine;
using System.Reflection;
using ItemChanger.Internal;
using ItemChanger;

namespace LoreMaster
{
    internal class EmbeddedSprite : ISprite
    {
        private const string _key = "Lore";

        private static SpriteManager _manager;

        static EmbeddedSprite()
        {
            _manager = new(Assembly.GetExecutingAssembly(), "LoreMaster.Resources.");
        }
        
        public Sprite Value 
            => _manager.GetSprite(_key);

        public ISprite Clone() 
            => new EmbeddedSprite();
    }
}
