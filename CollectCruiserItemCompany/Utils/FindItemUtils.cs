#nullable enable

using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using EasyTextEffects.Editor.MyBoxCopy.Extensions;
using UnityEngine;

namespace CollectCruiserItemCompany.Utils;

internal static class FindItemUtils
{
    internal static ManualLogSource Logger => CollectCruiserItemCompany.Logger!;

    internal static IEnumerable<GrabbableObject> GetAllItems()
    {
        var grabbableObjects = Object.FindObjectsOfType<GrabbableObject>() ?? throw new System.Exception("Failed to find GrabbableObject objects.");

        foreach (var obj in grabbableObjects)
        {
            if (obj == null)
            {
                continue;
            }

            yield return obj;
        }
    }

    internal static IEnumerable<GrabbableObject> GetAllItemsInCruiser()
    {
        var startOfRound = StartOfRound.Instance ?? throw new System.Exception("StartOfRound.Instance is null.");
        var elevatorTransform = startOfRound.elevatorTransform ?? throw new System.Exception("StartOfRound.Instance.elevatorTransform is null.");
        var shipTransform = ShipUtils.GetShipTransform() ?? throw new System.Exception("Ship transform is null.");
        var vehicleTransforms = ShipUtils.GetVehicleTransforms() ?? throw new System.Exception("Vehicle transforms is null.");

        var allItems = GetAllItems();

        var exclusionList = CollectCruiserItemCompany.ExclusionListConfig?.Value
            .Split(',', System.StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToArray()
            ?? [];

        Logger.LogInfo($"Exclusion list: {string.Join(", ", exclusionList)}");

        foreach (var item in allItems)
        {
            var itemProperties = item.itemProperties;
            if (itemProperties == null)
            {
                Logger.LogError($"Skipping item because itemProperties is null. name={item.name}");
                continue;
            }

            var itemName = itemProperties.itemName;
            var isInShipRoom = item.isInShipRoom;
            var isInElevator = item.isInElevator;
            var isHeld = item.isHeld;
            var isHeldByEnemy = item.isHeldByEnemy;
            var isPocketed = item.isPocketed;
            var parentTransformName = item.transform.parent?.name ?? "null";

            Logger.LogDebug(
                "Checking item." +
                $" itemName={itemName}" +
                $" isInShipRoom={isInShipRoom}" +
                $" isInElevator={isInElevator}" +
                $" isPocketed={isPocketed}" +
                $" isHeld={isHeld}" +
                $" isHeldByEnemy={isHeldByEnemy}" +
                $" parent={parentTransformName}"
            );

            if (isInShipRoom)
            {
                Logger.LogDebug($"Skipping item because it is in ship room. itemName={itemName}");
                continue;
            }

            // Handheld check
            if (isPocketed)
            {
                Logger.LogDebug($"Skipping item because it is pocketed. itemName={itemName}");
                continue;
            }

            if (isHeld)
            {
                Logger.LogDebug($"Skipping item because it is held. itemName={itemName}");
                continue;
            }

            if (isHeldByEnemy)
            {
                Logger.LogDebug($"Skipping item because it is held by enemy. itemName={itemName}");
                continue;
            }

            // Exclusion list check
            if (exclusionList.Contains(itemName))
            {
                Logger.LogDebug($"Skipping item because it is in the exclusion list. itemName={itemName}");
                continue;
            }

            // Boundary check
            if (item.transform.IsChildOf(elevatorTransform))
            {
                Logger.LogDebug($"Skipping item because it is on elevator. itemName={itemName}");
                continue;
            }

            if (item.transform.IsChildOf(shipTransform))
            {
                Logger.LogDebug($"Skipping item because it is on ship. itemName={itemName}");
                continue;
            }

            foreach (var vehicleTransform in vehicleTransforms)
            {
                if (vehicleTransform == null)
                {
                    continue;
                }

                if (!item.transform.IsChildOf(vehicleTransform))
                {
                    continue;
                }

                Logger.LogDebug($"Take item. itemName={itemName} vehicleParent={vehicleTransform.name}");
                yield return item;
            }

            Logger.LogDebug($"Skipping item because it is outside cruiser. itemName={itemName}");
        }
    }

    internal static IEnumerable<GrabbableObject> GetAllScraps()
    {
        var allItems = GetAllItemsInCruiser();

        foreach (var item in allItems)
        {
            var itemProperties = item.itemProperties;
            if (itemProperties == null)
            {
                continue;
            }

            if (itemProperties.isScrap)
            {
                yield return item;
            }
        }
    }

    internal static IEnumerable<GrabbableObject> GetAllTools()
    {
        var allItems = GetAllItemsInCruiser();

        foreach (var item in allItems)
        {
            var itemProperties = item.itemProperties;
            if (itemProperties == null)
            {
                continue;
            }

            if (!itemProperties.isScrap)
            {
                yield return item;
            }
        }
    }
}
