#nullable enable

using BepInEx.Logging;
using CollectCruiserItemCompany.Managers;
using HarmonyLib;

namespace CollectCruiserItemCompany.Patches;

[HarmonyPatch(typeof(Terminal))]
internal class TerminalPatch
{
    internal static ManualLogSource Logger => CollectCruiserItemCompany.Logger!;

    internal static string? RemovePunctuation(Terminal instance, string input)
    {
        var removePunctuationMethod = AccessTools.Method(typeof(Terminal), nameof(Terminal.RemovePunctuation));
        if (removePunctuationMethod == null)
        {
            Logger.LogError("Could not find Terminal.RemovePunctuation method.");
            return null;
        }

        try
        {
            var result = removePunctuationMethod.Invoke(instance, [input]);
            if (result is not string stringResult)
            {
                throw new System.Exception("Return value of Terminal.RemovePunctuation is not a string.");
            }

            return stringResult;
        }
        catch (System.Exception error)
        {
            Logger.LogError($"Error invoking Terminal.RemovePunctuation: {error}");
            return null;
        }
    }

    [HarmonyPatch(nameof(Terminal.ParsePlayerSentence))]
    [HarmonyPrefix]
    public static bool ParsePlayerSentencePrefix(Terminal __instance, ref TerminalNode __result)
    {
        var terminalCommandManager = CollectCruiserItemCompany.TerminalCommandManager;
        if (terminalCommandManager == null)
        {
            Logger.LogError("TerminalCommandManager is null.");
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

        sentence = RemovePunctuation(__instance, sentence);
        if (sentence == null)
        {
            Logger.LogError("Failed to remove punctuation from terminal input.");
            return true; // Use original method
        }

        var args = sentence.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

        var customTerminalNode = terminalCommandManager.ParseCommand(args);
        if (customTerminalNode == null)
        {
            return true; // Use original method
        }

        __result = customTerminalNode;
        return false; // Skip original method
    }
}
