using LoreMaster.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.SaveManagement;

public class LoreMasterLocalSaveData
{
    public Dictionary<string, PowerTag> Tags { get; set; } = new();

    public List<string> AcquiredPowersKey { get; set; } = new();
}
