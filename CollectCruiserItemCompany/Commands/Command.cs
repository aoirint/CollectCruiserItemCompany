#nullable enable

using BepInEx.Logging;

namespace CollectCruiserItemCompany.Commands;

internal sealed class ExecuteResult
{
    internal TerminalNode TerminalNode { get; }
    internal ConfirmableCommand? NextWaitingCommand { get; }

    internal ExecuteResult(TerminalNode terminalNode, ConfirmableCommand? nextWaitingCommand)
    {
        TerminalNode = terminalNode;
        NextWaitingCommand = nextWaitingCommand;
    }
}

internal abstract class Command
{
    internal static ManualLogSource Logger => CollectCruiserItemCompany.Logger!;

    internal abstract bool IsMatch(string[] args);

    internal ExecuteResult? Execute(string[] args)
    {
        var result = ExecuteCore(args);
        if (result == null)
        {
            Logger.LogError("ExecuteCore returned null.");
            return null;
        }

        ExecuteCorePostfix(result);

        return result;
    }

    internal abstract ExecuteResult? ExecuteCore(string[] args);

    internal virtual void ExecuteCorePostfix(ExecuteResult result)
    {
    }

    internal virtual void Reset()
    {
    }
}
