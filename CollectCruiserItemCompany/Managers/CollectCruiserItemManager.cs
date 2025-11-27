#nullable enable

using System.Collections;
using System.Collections.Generic;
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

    internal IEnumerator CollectToShipCoroutine(CollectType collectType)
    {
        if (!NetworkUtils.IsServer())
        {
            Logger.LogWarning("CollectToShipCoroutine called on client. Ignoring.");
            yield break;
        }

        Logger.LogInfo($"Starting CollectToShipCoroutine. collectType={collectType}.");

        var innerCoroutine = CollectToShipCoroutineInternal(collectType);
        while (true)
        {
            bool moveNext;
            try
            {
                moveNext = innerCoroutine.MoveNext();
            }
            catch (System.Exception error)
            {
                Logger.LogError($"Exception in CollectToShipCoroutineInternal: {error}");
                yield break;
            }

            if (!moveNext)
            {
                break;
            }

            yield return innerCoroutine.Current;
        }

        Logger.LogInfo("Finished CollectToShipCoroutine.");
    }

    internal IEnumerator<GrabbableObject> CollectToShipCoroutineInternal(CollectType collectType)
    {
        var startOfRound = StartOfRound.Instance ?? throw new System.Exception("StartOfRound.Instance is null.");
        var elevatorTransform = startOfRound.elevatorTransform ?? throw new System.Exception("StartOfRound.Instance.elevatorTransform is null.");

        // TODO: Use the RPC sender player
        var localPlayer = PlayerUtils.GetLocalPlayer();
        if (localPlayer == null)
        {
            throw new System.Exception("Local player is null.");
        }

        IEnumerable<GrabbableObject> items;
        if (collectType == CollectType.All)
        {
            // FIXME: Debugging workaround
            items = FindItemUtils.GetAllItems();
            // items = FindItemUtils.GetAllItemsInCruiser();
        }
        else if (collectType == CollectType.Scrap)
        {
            items = FindItemUtils.GetAllScraps();
        }
        else if (collectType == CollectType.Tool)
        {
            items = FindItemUtils.GetAllTools();
        }
        else
        {
            throw new System.Exception($"Unknown CollectType: {collectType}");
        }

        var worldBaseSpawnPosition = ShipUtils.GetBaseSpawnPosition() ?? throw new System.Exception("Base spawn position is null.");
        var localBaseSpawnPosition = elevatorTransform.InverseTransformPoint(worldBaseSpawnPosition);

        foreach (var item in TeleportItemUtils.TeleportItemsToShip(
            items,
            elevatorTransform,
            localBaseSpawnPosition,
            localPlayer
        ))
        {
            yield return item;
        }
    }
}
