#nullable enable

using System.Collections.Generic;
using BepInEx.Logging;
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

        foreach (var item in allItems)
        {
            var isInShipRoom = item.isInShipRoom;
            var isInElevator = item.isInElevator;
            var parentTransformName = item.transform.parent?.name ?? "null";

            Logger.LogDebug(
                "Checking item." +
                $" name={item.name}" +
                $" isInShipRoom={isInShipRoom}" +
                $" isInElevator={isInElevator}" +
                $" parent={parentTransformName}"
            );

            if (isInShipRoom)
            {
                Logger.LogDebug($"Skipping item because it is in ship room. name={item.name}");
                continue;
            }

            if (item.transform.IsChildOf(elevatorTransform))
            {
                Logger.LogDebug($"Skipping item because it is on elevator. name={item.name}");
                continue;
            }

            if (item.transform.IsChildOf(shipTransform))
            {
                Logger.LogDebug($"Skipping item because it is on ship. name={item.name}");
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

                Logger.LogDebug($"Take item. name={item.name} vehicleParent={vehicleTransform.name}");
                yield return item;
            }
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
