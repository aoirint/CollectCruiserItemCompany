#nullable enable

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using CollectCruiserItemCompany.Managers;

namespace CollectCruiserItemCompany;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Lethal Company.exe")]
public class CollectCruiserItemCompany : BaseUnityPlugin
{
    internal static new ManualLogSource? Logger { get; private set; }

    internal static Harmony harmony = new(MyPluginInfo.PLUGIN_GUID);

    internal static LandingHistoryManager LandingHistoryManager { get; } = new();

    internal static TerminalCommandManager TerminalCommandManager { get; } = new();

    internal static CollectCruiserItemManager CollectCruiserItemManager { get; } = new();

    private void Awake()
    {
        Logger = base.Logger;

        harmony.PatchAll();

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} is loaded!");
    }
}
