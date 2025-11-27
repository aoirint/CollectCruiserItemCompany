#nullable enable

using BepInEx.Logging;
using CollectCruiserItemCompany.Managers;
using CollectCruiserItemCompany.Utils;
using Unity.Netcode;

namespace CollectCruiserItemCompany.NetworkObjects;

internal class CollectionNetworkBehaviour : NetworkBehaviour
{
    internal static ManualLogSource Logger => CollectCruiserItemCompany.Logger!;

    [ServerRpc(RequireOwnership = false)]
    public void CollectRequestServerRpc(
        CollectType collectType,
        ServerRpcParams rpcParams = default
    )
    {
        if (!NetworkUtils.IsServer())
        {
            Logger.LogError("CollectRequestServerRpc called on client. Ignoring.");
            return;
        }

        Logger.LogInfo("Received CollectRequestServerRpc from client.");
        var collectCruiserItemManager = CollectCruiserItemCompany.CollectCruiserItemManager;
        if (collectCruiserItemManager == null)
        {
            Logger.LogError("CollectCruiserItemManager is null. Cannot process collect request.");
            return;
        }

        var senderClientId = rpcParams.Receive.SenderClientId;
        var senderIsHost = senderClientId == 0u; // In Unity Netcode, host has ClientId 0

        var permission = CollectCruiserItemCompany.PermissionConfig?.Value ?? Permission.HostOnly;
        Logger.LogDebug($"Current permission setting: {permission}");

        if (permission == Permission.HostOnly && !senderIsHost)
        {
            Logger.LogInfo("Cruiser collection is requested but cancelled. Permission is HostOnly and sender is not the host.");
            return;
        }

        StartCoroutine(
            collectCruiserItemManager.CollectToShipCoroutine(
                collectType: collectType
            )
        );
    }
}
