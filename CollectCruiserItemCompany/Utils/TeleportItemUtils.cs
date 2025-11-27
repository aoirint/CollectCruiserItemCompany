#nullable enable

using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using CollectCruiserItemCompany.Extensions;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace CollectCruiserItemCompany.Utils;

internal enum ClientTeleportMethod
{
    Throw,
    Place
}

internal static class TeleportItemUtils
{
    internal static ManualLogSource Logger => CollectCruiserItemCompany.Logger!;

    private static void ThrowObjectClientRpc(
        PlayerControllerB instance,
        bool droppedInElevator,
        bool droppedInShipRoom,
        Vector3 targetFloorPosition,
        NetworkObjectReference grabbedObject,
        int floorYRot
    )
    {
        var throwObjectClientRpcMethod = AccessTools.Method(typeof(PlayerControllerB), nameof(PlayerControllerB.ThrowObjectClientRpc));
        if (throwObjectClientRpcMethod == null)
        {
            Logger.LogError("Could not find PlayerControllerB.ThrowObjectClientRpc method.");
            return;
        }

        try
        {
            throwObjectClientRpcMethod.Invoke(
                instance,
                [
                    droppedInElevator,
                    droppedInShipRoom,
                    targetFloorPosition,
                    grabbedObject,
                    floorYRot
                ]
            );
        }
        catch (System.Exception error)
        {
            Logger.LogError($"Error invoking PlayerControllerB.ThrowObjectClientRpc: {error}");
            return;
        }
    }

    private static void TeleportItemForOwnerInternal(
        GrabbableObject item,
        Transform newParentTransform,
        Vector3 newLocalPosition,
        PlayerControllerB localPlayer
    )
    {
        item.transform.SetParent(newParentTransform);
        item.transform.localPosition = newLocalPosition;
        item.transform.rotation = Quaternion.identity;

        // Prevent the item teleports to the old position due to fake falling.
        // NOTE: These positions are local positions.
        item.fallTime = 0f;
        item.startFallingPosition = newLocalPosition;
        item.targetFloorPosition = newLocalPosition;

        // Set the item to follow the parent transform and keep after the current day.
        // Play the item collection animation if not already played.
        localPlayer.SetItemInElevator(
            true, // droppedInShipRoom
            true, // droppedInElevator
            item // gObject
        );

        // NOTE: Do not update hasHitGround. This would desync the easter egg explosion.
        // TODO: Find a way to sync hasHitGround using vanilla Client RPCs.
        // item.hasHitGround = false;
    }

    internal static IEnumerable<GrabbableObject> TeleportItemsToShip(
        IEnumerable<GrabbableObject> items,
        Transform newParentTransform,
        Vector3 localPosition,
        PlayerControllerB localPlayer
    )
    {
        const float offsetXRange = 0.7f;
        const float offsetY = 0.5f;
        const float offsetZ = 2.0f;

        uint seed = System.BitConverter.ToUInt32(System.Guid.NewGuid().ToByteArray(), 0);
        if (seed == 0u) seed = 1u;
        var random = new Unity.Mathematics.Random(seed);

        // Chunked calculation of new positions
        const int maxChunkSize = 20;
        foreach (var chunkItems in items.Chunk(maxChunkSize))
        {
            var chunkSize = chunkItems.Count();

            // localNewItemAirPositions
            Vector3[] positions = [..
                chunkItems.Select(
                    offsetVector =>
                        localPosition +
                        new Vector3(random.NextFloat(-offsetXRange, offsetXRange), offsetY, offsetZ)
                )
            ];

            // inplace: worldNewItemAirPositions
            newParentTransform.TransformPoints(positions);

            // inplace: worldNewItemPositions
            positions = [.. chunkItems.Select((item, index) => item.GetItemFloorPosition(positions[index]))];

            // clone
            var worldNewItemPositions = positions.ToArray();

            // inplace: localNewItemPositions
            newParentTransform.InverseTransformPoints(positions);

            foreach (var (item, index) in chunkItems.Select((item, index) => (item, index)))
            {
                var worldOldItemPosition = item.transform.position;
                var localNewItemPosition = positions[index];
                var worldNewItemPosition = worldNewItemPositions[index];

                Logger.LogInfo(
                    "Teleporting item." +
                    $" name={item.name}" +
                    $" worldOldItemPosition=({worldOldItemPosition.x:F2}, {worldOldItemPosition.y:F2}, {worldOldItemPosition.z:F2})" +
                    $" localNewItemPosition=({localNewItemPosition.x:F2}, {localNewItemPosition.y:F2}, {localNewItemPosition.z:F2})" +
                    $" worldNewItemPosition=({worldNewItemPosition.x:F2}, {worldNewItemPosition.y:F2}, {worldNewItemPosition.z:F2})"
                );

                // TODO: Simulate the easter egg explosion logic in GrabbableObject.EquipItem based on the old position.
                // NOTE: This would not work because hasHitGround does not get synced.
                // if (item is StunGrenadeItem stunGrenadeItem)
                // {
                //     Logger.LogDebug($"Simulating easter egg explosion logic before teleporting. name={item.name}");
                //     stunGrenadeItem.SetExplodeOnThrowServerRpc();
                // }

                // NOTE: ThrowObjectClientRpc skips the teleport for the owner of the network object. So we need to teleport for the owner in another way.
                TeleportItemForOwnerInternal(
                    item,
                    newParentTransform,
                    localNewItemPosition,
                    localPlayer
                );

                ThrowObjectClientRpc(
                    localPlayer, // instance
                    true, // droppedInElevator
                    true, // droppedInShipRoom
                    localNewItemPosition, // targetFloorPosition
                    item.NetworkObject, // grabbedObject
                    -1 // floorYRot
                );

                yield return item;
            }
        }
    }
}
