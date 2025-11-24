#nullable enable

using BepInEx.Logging;

namespace CollectCruiserItemCompany.Commands;

internal abstract class Command
{
    internal static ManualLogSource Logger => CollectCruiserItemCompany.Logger!;

    internal abstract bool IsMatch(string[] args);

    internal virtual TerminalNode? ExecutePrefix(string[] args)
    {
        return null;
    }

    internal TerminalNode Execute(string[] args)
    {
        var terminalNode = ExecutePrefix(args);
        if (terminalNode != null)
        {
            return terminalNode;
        }

        terminalNode = ExecuteCore(args);

        ExecuteCorePostfix(terminalNode);

        return terminalNode;
    }

    internal virtual void ExecutePrefixPostfix(TerminalNode terminalNode)
    {
    }

    internal virtual void ExecuteCorePostfix(TerminalNode terminalNode)
    {
    }

    internal abstract TerminalNode ExecuteCore(string[] args);
}
