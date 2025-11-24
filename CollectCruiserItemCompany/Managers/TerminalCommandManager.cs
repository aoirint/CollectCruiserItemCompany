#nullable enable

using BepInEx.Logging;
using CollectCruiserItemCompany.Commands;

namespace CollectCruiserItemCompany.Managers;

internal class TerminalCommandManager
{
    internal static ManualLogSource Logger => CollectCruiserItemCompany.Logger!;

    internal static Command[] COMMANDS { get; } = [
        new CollectScrapCommand(),
        new HelpCommand()
    ];

    private ConfirmableCommand? waitingCommand = null;

    internal TerminalNode? ParseCommand(string[] args)
    {
        if (waitingCommand != null)
        {
            var terminalNode = waitingCommand.Execute(args);

            ClearWaitingCommand();

            return terminalNode;
        }

        foreach (var command in COMMANDS)
        {
            if (command.IsMatch(args))
            {
                var terminalNode = command.Execute(args);

                if (command is ConfirmableCommand confirmableCommand)
                {
                    waitingCommand = confirmableCommand;
                }

                return terminalNode;
            }
        }

        return null;
    }

    internal void ClearWaitingCommand()
    {
        waitingCommand = null;
    }
}
