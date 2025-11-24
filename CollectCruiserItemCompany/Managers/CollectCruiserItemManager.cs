#nullable enable

using System.Collections;
using BepInEx.Logging;
using CollectCruiserItemCompany.Utils;

namespace CollectCruiserItemCompany.Managers;

internal enum CollectType
{
    All,
    Scrap,
    Tool
}

internal class CollectCruiserItemManager
{
    internal static ManualLogSource Logger => CollectCruiserItemCompany.Logger!;

    internal GrabbableObject[]? GetAllItems()
    {
        return null;
    }

    internal GrabbableObject[]? GetAllScraps()
    {
        return null;
    }

    internal GrabbableObject[]? GetAllTools()
    {
        return null;
    }

    internal IEnumerator CollectToShipCoroutine(CollectType collectType)
    {
        if (!NetworkUtils.IsServer())
        {
            Logger.LogWarning("CollectToShipCoroutine called on client. Ignoring.");
            yield break;
        }

        Logger.LogInfo($"Starting CollectToShipCoroutine. collectType={collectType}.");

        GrabbableObject[]? itemsNullable;
        if (collectType == CollectType.All)
        {
            itemsNullable = GetAllItems();
        }
        else if (collectType == CollectType.Scrap)
        {
            itemsNullable = GetAllScraps();
        }
        else if (collectType == CollectType.Tool)
        {
            itemsNullable = GetAllTools();
        }
        else
        {
            Logger.LogError($"Unknown CollectType. collectType={collectType}.");
            yield break;
        }

        if (itemsNullable == null)
        {
            Logger.LogError("Failed to get items for collection.");
            yield break;
        }
        var items = itemsNullable;

        // TODO: implement item collection logic
    }
}
