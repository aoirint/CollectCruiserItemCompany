#nullable enable

using BepInEx.Logging;

namespace CollectCruiserItemCompany.Commands;

internal abstract class Command
{
    internal static ManualLogSource Logger => CollectCruiserItemCompany.Logger!;

    internal abstract bool IsMatch(string[] args);

    internal abstract TerminalNode Execute(string[] args);
}
