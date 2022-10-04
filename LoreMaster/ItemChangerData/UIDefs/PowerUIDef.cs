using ItemChanger;
using ItemChanger.UIDefs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.ItemChangerData.UIDefs;

internal class PowerUIDef : LoreUIDef
{
    public override void SendMessage(MessageType type, Action callback)
    {
        base.SendMessage(type, callback);
    }
}
