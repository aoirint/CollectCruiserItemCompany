#nullable enable

using BepInEx.Logging;
using HarmonyLib;

namespace CollectCruiserItemCompany.Patches;

[HarmonyPatch(typeof(Terminal))]
internal class TerminalPatch
{
    internal static ManualLogSource Logger => CollectCruiserItemCompany.Logger!;

    [HarmonyPatch(nameof(Terminal.ParsePlayerSentence))]
    [HarmonyPrefix]
    public static bool ParsePlayerSentencePrefix(Terminal __instance, ref TerminalNode __result)
    {
        var collectCruiserItemManager = CollectCruiserItemCompany.CollectCruiserItemManager;
        if (collectCruiserItemManager == null)
        {
            Logger.LogError("CollectCruiserItemManager is null.");
            return true;
        }

        // TODO: Implement custom logic here. Return false to skip original method.
        return true;
    }
}
