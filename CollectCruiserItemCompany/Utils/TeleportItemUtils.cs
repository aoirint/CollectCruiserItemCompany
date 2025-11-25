#nullable enable

using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using GameNetcodeStuff;
using UnityEngine;

namespace CollectCruiserItemCompany.Utils;

internal static class TeleportItemUtils
{
    internal static ManualLogSource Logger => CollectCruiserItemCompany.Logger!;

    internal static IEnumerable<GrabbableObject> TeleportItems(
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

                item.transform.SetParent(newParentTransform);
                item.transform.position = worldNewItemPosition;
                item.transform.rotation = Quaternion.identity;

                // Prevent the item teleports to the old position due to fake falling.
                // NOTE: These positions are local positions.
                item.fallTime = 0f;
                item.startFallingPosition = localNewItemPosition;
                item.targetFloorPosition = localNewItemPosition;

                // Set the item to follow the parent transform and keep after the current day.
                // Play the item collection animation if not already played.
                localPlayer.SetItemInElevator(
                    true, // droppedInShipRoom
                    true, // droppedInElevator
                    item // gObject
                );

                // Disable drop sound effect.
                item.hasHitGround = true;

                yield return item;
            }
        }
    }
}
