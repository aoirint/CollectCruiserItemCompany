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
    public void CollectRequestServerRpc(CollectType collectType)
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

        StartCoroutine(
            collectCruiserItemManager.CollectToShipCoroutine(
                collectType: collectType
            )
        );
    }
}
