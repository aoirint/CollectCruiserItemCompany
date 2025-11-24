#nullable enable

using System.Text;
using BepInEx.Logging;
using CollectCruiserItemCompany.Commands;
using CollectCruiserItemCompany.Utils;

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
            var result = waitingCommand.ExecuteConfirmation(args);
            if (result == null)
            {
                Logger.LogError("ExecuteConfirmation returned null.");

                // Reset internal state
                waitingCommand = null;

                return CreateInvalidStateNode();
            }

            waitingCommand = result.NextWaitingCommand;

            return result.TerminalNode;
        }

        foreach (var command in COMMANDS)
        {
            if (command.IsMatch(args))
            {
                var result = command.Execute(args);
                if (result == null)
                {
                    Logger.LogError("Execute returned null.");
                    return CreateInvalidStateNode();
                }

                waitingCommand = result.NextWaitingCommand;

                return result.TerminalNode;
            }
        }

        return null;
    }

    internal TerminalNode CreateInvalidStateNode()
    {
        var builder = new StringBuilder();

        builder.AppendLine("Invalid state.");

        return TerminalUtils.CreateTerminalNode(
            displayText: builder.ToString(),
            clearPreviousText: false
        );
    }

    internal void ClearWaitingCommand()
    {
        waitingCommand = null;
    }
}
