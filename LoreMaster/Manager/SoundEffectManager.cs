using ItemChanger.Internal;
using System.Reflection;

namespace LoreMaster.Manager;

internal static class SoundEffectManager
{
    public static SoundManager Manager { get; set; }

    static SoundEffectManager()
    {
        Manager = new(Assembly.GetExecutingAssembly(), "LoreMaster.Resources.Sounds.");
    }
}
