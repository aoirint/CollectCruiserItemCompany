#nullable enable

using BepInEx.Logging;
using CollectCruiserItemCompany.Commands;

namespace CollectCruiserItemCompany.Managers;

internal static class TerminalCommandHelpers
{
    internal static ManualLogSource Logger => CollectCruiserItemCompany.Logger!;

    internal static Command[] COMMANDS { get; } = [
        new HelpCommand()
    ];

    internal static TerminalNode? ParseCommand(string[] args)
    {
        foreach (var command in COMMANDS)
        {
            if (command.IsMatch(args))
            {
                return command.Execute(args);
            }
        }

        return null;
    }
}
