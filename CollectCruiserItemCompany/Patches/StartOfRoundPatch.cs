#nullable enable

using BepInEx.Logging;
using CollectCruiserItemCompany.NetworkObjects;
using HarmonyLib;

namespace CollectCruiserItemCompany.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal class StartOfRoundPatch
{
    internal static ManualLogSource Logger => CollectCruiserItemCompany.Logger!;

    [HarmonyPatch(nameof(StartOfRound.Awake))]
    [HarmonyPostfix]
    public static void AwakePrefix(StartOfRound __instance)
    {
        var gameObject = __instance.gameObject;
        if (gameObject == null)
        {
            Logger.LogError("StartOfRound.gameObject is null.");
            return;
        }

        __instance.gameObject.AddComponent<CollectionNetworkBehaviour>();
    }
}
