#nullable enable

using BepInEx.Logging;
using GameNetcodeStuff;

namespace CollectCruiserItemCompany.Utils;

internal static class PlayerUtils
{
    internal static ManualLogSource Logger => CollectCruiserItemCompany.Logger!;

    internal static PlayerControllerB? GetLocalPlayer()
    {
        var gameNetworkManager = GameNetworkManager.Instance;
        if (gameNetworkManager == null)
        {
            Logger.LogError("GameNetworkManager.Instance is null.");
            return null;
        }

        var localPlayer = gameNetworkManager.localPlayerController;
        if (localPlayer == null)
        {
            Logger.LogError("GameNetworkManager.Instance.localPlayerController is null.");
            return null;
        }

        return localPlayer;
    }
}
