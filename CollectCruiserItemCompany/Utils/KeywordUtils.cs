#nullable enable

using BepInEx.Logging;

namespace CollectCruiserItemCompany.Utils;

internal static class KeywordUtils
{
    internal static ManualLogSource Logger => CollectCruiserItemCompany.Logger!;

    internal static bool IsCollectCommand(string keyword)
    {
        keyword = keyword.ToLower();

        if (keyword == "collect" || keyword == "colect")
        {
            return true;
        }

        return false;
    }

    internal static bool IsScrapArgument(string keyword)
    {
        keyword = keyword.ToLower();

        var firstCharacter = keyword.Length > 0 ? keyword[..1] : "";

        if (firstCharacter == "s")
        {
            return true;
        }

        return false;
    }
}
