#nullable enable

using BepInEx.Logging;
using CollectCruiserItemCompany.Managers;
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
            return true; // Use original method
        }

        var screenTextInputField = __instance.screenText;
        if (screenTextInputField == null)
        {
            Logger.LogError("Terminal.screenText is null.");
            return true; // Use original method
        }

        var textAdded = __instance.textAdded;

        var sentence = screenTextInputField.text;
        sentence = sentence.Substring(sentence.Length - textAdded);
        sentence = __instance.RemovePunctuation(sentence);

        var args = sentence.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

        var customTerminalNode = TerminalCommandHelpers.ParseCommand(args);
        if (customTerminalNode == null)
        {
            return true; // Use original method
        }

        __result = customTerminalNode;
        return false; // Skip original method
    }
}
