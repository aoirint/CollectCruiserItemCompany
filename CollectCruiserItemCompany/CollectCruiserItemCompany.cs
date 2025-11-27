#nullable enable

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using CollectCruiserItemCompany.Managers;
using BepInEx.Configuration;

namespace CollectCruiserItemCompany;

public enum Permission
{
    HostOnly,
    Everone
}

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Lethal Company.exe")]
public class CollectCruiserItemCompany : BaseUnityPlugin
{
    internal static new ManualLogSource? Logger { get; private set; }

    internal static Harmony harmony = new(MyPluginInfo.PLUGIN_GUID);

    internal static LandingHistoryManager LandingHistoryManager { get; } = new();

    internal static TerminalCommandManager TerminalCommandManager { get; } = new();

    internal static CollectCruiserItemManager CollectCruiserItemManager { get; } = new();

    internal static ConfigEntry<Permission>? PermissionConfig { get; private set; }

    private void Awake()
    {
        Logger = base.Logger;

        PermissionConfig = Config.Bind(
            "General",
            "Permission",
            Permission.HostOnly,
            "Controls who can collect items from cruiser by terminal." +
            " If HostOnly, only the host can collect items." +
            " If Everyone, all players can collect items if they have installed this mod."
        );

        harmony.PatchAll();

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} is loaded!");
    }
}
