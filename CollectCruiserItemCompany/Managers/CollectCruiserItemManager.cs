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

        var elevatorTransform = startOfRound.elevatorTransform;
        if (elevatorTransform == null)
        {
            throw new System.Exception("StartOfRound.Instance.elevatorTransform is null.");
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

            items.Add(obj);
            continue;

            var isInShipRoom = obj.isInShipRoom;
            var isInElevator = obj.isInElevator;
            var parentTransformName = obj.transform.parent?.name ?? "null";

            Logger.LogDebug(
                $"Checking object: {obj.name}, isInShipRoom={isInShipRoom}, isInElevator={isInElevator}, " +
                $"parent={parentTransformName}"
            );

            if (isInShipRoom)
            {
                Logger.LogDebug($"Skipping object because it is in ship room. name={obj.name}");
                continue;
            }

            if (obj.transform.IsChildOf(elevatorTransform))
            {
                Logger.LogDebug($"Skipping object because it is on elevator. name={obj.name}");
                continue;
            }

            if (obj.transform.IsChildOf(shipTransform))
            {
                Logger.LogDebug($"Skipping object because it is on ship. name={obj.name}");
                continue;
            }

            foreach (var vehicleTransform in vehicleTransforms)
            {
                if (vehicleTransform == null)
                {
                    continue;
                }

                if (!obj.transform.IsChildOf(vehicleTransform))
                {
                    continue;
                }

                Logger.LogDebug($"Adding object. name={obj.name} vehicleParent={vehicleTransform.name}");
                items.Add(obj);
            }
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

        var elevatorTransform = startOfRound.elevatorTransform;
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

        var worldBaseSpawnPosition = ShipUtils.GetBaseSpawnPosition();
        if (worldBaseSpawnPosition == null)
        {
            throw new System.Exception("Base spawn position is null.");
        }

        var localBaseSpawnPosition = elevatorTransform.InverseTransformPoint(worldBaseSpawnPosition.Value);

        const float offsetXRange = 0.7f;
        const float offsetY = 0.5f;
        const float offsetZ = 2.0f;

        uint seed = System.BitConverter.ToUInt32(System.Guid.NewGuid().ToByteArray(), 0);
        if (seed == 0) seed = 1;
        var random = new Unity.Mathematics.Random(seed);

        // Chunked calculation of spawn positions
        const int maxChunkSize = 20;
        foreach (var chunkItems in items.Chunk(maxChunkSize))
        {
            var chunkSize = chunkItems.Count();

            // localNewItemAirPositions
            Vector3[] positions = [..
                chunkItems.Select(
                    offsetVector =>
                        localBaseSpawnPosition +
                        new Vector3(random.NextFloat(-offsetXRange, offsetXRange), offsetY, offsetZ)
                )
            ];

            // worldNewItemAirPositions
            elevatorTransform.TransformPoints(positions);

            // worldNewItemPositions
            positions = [.. chunkItems.Select((item, index) => item.GetItemFloorPosition(positions[index]))];

            // localNewItemPositions
            elevatorTransform.InverseTransformPoints(positions);

            foreach (var (item, index) in chunkItems.Select((item, index) => (item, index)))
            {
                var worldOldItemPosition = item.transform.position;
                var localNewItemPosition = positions[index];
                var worldNewItemPosition = elevatorTransform.TransformPoint(localNewItemPosition);

                Logger.LogInfo(
                    "Collecting item." +
                    $" name={item.name}" +
                    $" worldOldItemPosition=({worldOldItemPosition.x:F2}, {worldOldItemPosition.y:F2}, {worldOldItemPosition.z:F2})" +
                    $" localNewItemPosition=({localNewItemPosition.x:F2}, {localNewItemPosition.y:F2}, {localNewItemPosition.z:F2})"
                );

                item.transform.SetParent(elevatorTransform);
                item.transform.position = worldNewItemPosition;
                item.transform.rotation = Quaternion.identity;

                // Set parameters to start the item falls
                // NOTE: These positions are local positions.
                item.fallTime = 0f;
                item.startFallingPosition = localNewItemPosition;
                item.targetFloorPosition = localNewItemPosition;

                item.isInElevator = true;
                item.isInShipRoom = true;
                item.hasHitGround = true;

                yield return item;
            }
        }
    }
}
