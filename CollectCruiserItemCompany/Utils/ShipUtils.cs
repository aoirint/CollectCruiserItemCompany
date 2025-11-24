#nullable enable

using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using UnityEngine;

namespace CollectCruiserItemCompany.Utils;

internal static class ShipUtils
{
    internal static ManualLogSource Logger => CollectCruiserItemCompany.Logger!;

    private static GameObject? cachedShipObject = null;

    public static GameObject? GetShipObject()
    {
        if (cachedShipObject != null)
        {
            return cachedShipObject;
        }

        var shipObject = GameObject.Find("Environment/HangarShip");
        if (shipObject == null)
        {
            Logger.LogError("Failed to find ship object in scene.");
            return null;
        }

        cachedShipObject = shipObject;
        return shipObject;
    }

    public static Transform? GetShipTransform()
    {
        var shipObject = GetShipObject();
        if (shipObject == null)
        {
            Logger.LogError("Ship object is null. Cannot get transform.");
            return null;
        }

        var shipTransform = shipObject.transform;
        if (shipTransform == null)
        {
            Logger.LogError("Ship transform is null.");
            return null;
        }

        return shipTransform;
    }

    public static Transform[]? GetVehicleTransforms()
    {
        var vehicleControllers = Object.FindObjectsOfType<VehicleController>();
        if (vehicleControllers == null)
        {
            Logger.LogError("Failed to find VehicleController objects.");
            return null;
        }

        List<Transform> vehicleTransforms = [];
        foreach (var vehicleController in vehicleControllers)
        {
            var transform = vehicleController.transform;
            if (transform == null)
            {
                Logger.LogError("VehicleController.transform is null.");
                return null;
            }

            vehicleTransforms.Add(transform);
        }

        return [.. vehicleTransforms];
    }
    public static Vector3? GetBaseSpawnPosition()
    {
        var startOfRound = StartOfRound.Instance;
        if (startOfRound == null)
        {
            // Invalid state
            Logger.LogError("StartOfRound.Instance is null.");
            return null;
        }

        var playerSpawnPositions = startOfRound.playerSpawnPositions;
        if (playerSpawnPositions == null)
        {
            // Invalid state
            Logger.LogError("StartOfRound.Instance.playerSpawnPositions is null.");
            return null;
        }

        var playerSpawnPositionTransform = playerSpawnPositions.ElementAtOrDefault(1);
        if (playerSpawnPositionTransform == null)
        {
            // Invalid state
            Logger.LogError("Player spawn position is null for ID 1.");
            return null;
        }

        return playerSpawnPositionTransform.position;
    }
}
