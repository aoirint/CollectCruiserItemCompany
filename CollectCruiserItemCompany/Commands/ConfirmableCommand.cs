#nullable enable

using System.Linq;

namespace CollectCruiserItemCompany.Commands;

internal abstract class ConfirmableCommand : Command
{
    private TerminalNode? previousNode = null;

    internal abstract ExecuteResult ExecuteConfirm();

    internal abstract ExecuteResult ExecuteDeny();

    internal virtual ExecuteResult? ExecuteInvalid()
    {
        if (previousNode == null)
        {
            Logger.LogError("previousNode is null.");
            return null;
        }

        return new ExecuteResult(
            terminalNode: previousNode,
            nextWaitingCommand: this
        );
    }

    internal ExecuteResult? ExecuteConfirmation(string[] args)
    {
        var result = ExecuteConfirmationCore(args);
        if (result == null)
        {
            Logger.LogError("ExecuteConfirmationCore returned null.");

            // Reset internal state
            previousNode = null;

            return null;
        }

        previousNode = result.TerminalNode;

        return result;
    }

    internal ExecuteResult? ExecuteConfirmationCore(string[] args)
    {
        var isConfirmed = IsConfirmed(args);

        if (isConfirmed == true)
        {
            return ExecuteConfirm();
        }
        else if (isConfirmed == false)
        {
            return ExecuteDeny();
        }

        return ExecuteInvalid();
    }

    internal override void ExecuteCorePostfix(ExecuteResult result)
    {
        previousNode = result.TerminalNode;
    }

    internal bool? IsConfirmed(string[] args)
    {
        args = args.Select(arg => arg.ToLower()).ToArray();

        if (args.Length == 0)
        {
            return null;
        }

        var arg0 = args[0];
        var firstCharacter = arg0.Length > 0 ? arg0[..1] : string.Empty;

        if (firstCharacter == "c")
        {
            return true;
        }
        else if (firstCharacter == "d")
        {
            return false;
        }
        else
        {
            return null;
        }
    }
}
