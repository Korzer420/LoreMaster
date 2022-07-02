using GlobalEnums;
using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using Modding;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster
{
    class TouristMagnetItem : AbstractItem
    {
        private bool _toTemple;
        private string _name = "Lumafly Express";

        public TouristMagnetItem(bool toTemple, string itemName)
        {
            _toTemple = toTemple;
            name = itemName;
            UIDef = new MsgUIDef()
            {
                name = new BoxedString(_name),
                shopDesc = new BoxedString(_name),
                sprite = new EmbeddedSprite()
            };
            tags = new List<Tag>()
            {
                new PersistentItemTag() { Persistence = Persistence.Persistent},
                new CompletionWeightTag() { Weight = 0}
            };
        }

        public override void GiveImmediate(GiveInfo info)
        {
            //HeroController.instance.StartCoroutine(Travel());
        }


       
    }
}
