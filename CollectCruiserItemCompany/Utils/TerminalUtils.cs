#nullable enable

using BepInEx.Logging;
using UnityEngine;

namespace CollectCruiserItemCompany.Utils;

internal static class TerminalUtils
{
    internal static ManualLogSource Logger => CollectCruiserItemCompany.Logger!;

    public static TerminalNode CreateTerminalNode(string displayText, bool clearPreviousText)
    {
        var terminalNode = ScriptableObject.CreateInstance<TerminalNode>();

        terminalNode.displayText = displayText;
        terminalNode.clearPreviousText = clearPreviousText;

        return terminalNode;
    }
}
