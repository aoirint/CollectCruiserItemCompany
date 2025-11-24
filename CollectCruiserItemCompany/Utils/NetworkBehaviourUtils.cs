#nullable enable

using BepInEx.Logging;
using CollectCruiserItemCompany.NetworkObjects;

namespace CollectCruiserItemCompany.Utils;

internal static class NetworkBehaviourUtils
{
    internal static ManualLogSource Logger => CollectCruiserItemCompany.Logger!;

    private static CollectionNetworkBehaviour? cachedCollectionNetworkBehaviour;

    public static CollectionNetworkBehaviour? GetCollectionNetworkBehaviour()
    {
        if (cachedCollectionNetworkBehaviour != null)
        {
            return cachedCollectionNetworkBehaviour;
        }

        var startOfRound = StartOfRound.Instance;
        if (startOfRound == null)
        {
            Logger.LogError("StartOfRound.Instance is null.");
            return null;
        }

        var collectionNetworkBehaviour = startOfRound.GetComponent<CollectionNetworkBehaviour>();
        if (collectionNetworkBehaviour == null)
        {
            Logger.LogError("CollectionNetworkBehaviour component not found on StartOfRound instance.");
            return null;
        }

        cachedCollectionNetworkBehaviour = collectionNetworkBehaviour;

        return collectionNetworkBehaviour;
    }
}
