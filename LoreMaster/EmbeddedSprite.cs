using UnityEngine;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

using ItemChanger.Internal;
using ItemChanger;

namespace LoreMaster
{
    internal class EmbeddedSprite : ISprite
    {
        private string _key;

        private static SpriteManager _manager;

        static EmbeddedSprite()
        {
            _manager = new(Assembly.GetExecutingAssembly(), "Resources");
        }

        public EmbeddedSprite(string key) 
            => _key = key;
        
        public Sprite Value 
            => SpriteManager.Instance.GetSprite(_key);

        public ISprite Clone() 
            => new EmbeddedSprite(_key);
    }
}
