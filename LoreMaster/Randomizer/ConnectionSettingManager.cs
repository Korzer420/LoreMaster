using ConnectionSettingsCode;
using MenuChanger;
using MenuChanger.MenuElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.Randomizer;

/// <summary>
/// Handler for creating/inserting setting codes.
/// </summary>
public static class ConnectionSettingManager
{
    public static void CreateSettingsCode(MenuPage menuPage, IEnumerable<IValueElement> valueElements)
    {
        new SettingsCode(menuPage, LoreMaster.Instance, valueElements);
    }
}
