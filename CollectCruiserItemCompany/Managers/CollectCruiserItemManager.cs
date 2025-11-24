#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using CollectCruiserItemCompany.Utils;
using UnityEngine;

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

    internal IEnumerable<GrabbableObject> GetAllItemsInCruiser()
    {
        var startOfRound = StartOfRound.Instance;
        if (startOfRound == null)
        {
            throw new System.Exception("StartOfRound.Instance is null.");
        }

        var shipTransform = ShipUtils.GetShipTransform();
        if (shipTransform == null)
        {
            throw new System.Exception("Ship transform is null.");
        }


        var vehicleTransforms = ShipUtils.GetVehicleTransforms();
        if (vehicleTransforms == null)
        {
            throw new System.Exception("Vehicle transforms is null.");
        }

        var grabbableObjects = Object.FindObjectsOfType<GrabbableObject>();
        if (grabbableObjects == null)
        {
            throw new System.Exception("Failed to find GrabbableObject objects.");
        }

        List<GrabbableObject> items = [];
        foreach (var obj in grabbableObjects)
        {
            if (obj == null)
            {
                continue;
            }

            if (!obj.isInShipRoom)
            {
                continue;
            }

            if (obj.transform.IsChildOf(shipTransform))
            {
                continue;
            }

            var isInCruiser = vehicleTransforms.Any(transform => obj.transform.IsChildOf(transform));
            if (!isInCruiser)
            {
                continue;
            }

            items.Add(obj);
        }

        return [.. items];
    }

    internal IEnumerable<GrabbableObject> GetAllScraps()
    {
        var allItems = GetAllItemsInCruiser();
        var scraps = allItems.Where(item => item.itemProperties != null && item.itemProperties.isScrap);
        return scraps;
    }

    internal IEnumerable<GrabbableObject> GetAllTools()
    {
        var allItems = GetAllItemsInCruiser();
        var tools = allItems.Where(item => item.itemProperties != null && !item.itemProperties.isScrap);
        return tools;
    }

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
        var startOfRound = StartOfRound.Instance;
        if (startOfRound == null)
        {
            throw new System.Exception("StartOfRound.Instance is null.");
        }

        var elevatorTransform = startOfRound.elevatorTransform; ;
        if (elevatorTransform == null)
        {
            throw new System.Exception("StartOfRound.Instance.elevatorTransform is null.");
        }

        IEnumerable<GrabbableObject> items;
        if (collectType == CollectType.All)
        {
            items = GetAllItemsInCruiser();
        }
        else if (collectType == CollectType.Scrap)
        {
            items = GetAllScraps();
        }
        else if (collectType == CollectType.Tool)
        {
            items = GetAllTools();
        }
        else
        {
            throw new System.Exception($"Unknown CollectType: {collectType}");
        }

        var baseSpawnPosition = ShipUtils.GetBaseSpawnPosition();
        if (baseSpawnPosition == null)
        {
            throw new System.Exception("Base spawn position is null.");
        }

        const float offsetXRange = 0.7f;
        const float offsetY = 0.5f;
        const float offsetZ = 2.0f;

        foreach (var item in items)
        {
            Logger.LogInfo($"Collecting item: {item.name}");

            var offsetX = Random.Range(-offsetXRange, offsetXRange);

            item.transform.SetParent(elevatorTransform);
            item.transform.localPosition = baseSpawnPosition.Value + new Vector3(offsetX, offsetY, offsetZ);

            yield return item;
        }
    }
}
