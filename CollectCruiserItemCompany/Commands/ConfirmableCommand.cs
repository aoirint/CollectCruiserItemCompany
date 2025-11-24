#nullable enable

using System.Linq;
using System.Text;
using CollectCruiserItemCompany.Utils;

namespace CollectCruiserItemCompany.Commands;

internal abstract class ConfirmableCommand : Command
{
    internal TerminalNode? waitingTerminalNode = null;

    internal abstract TerminalNode ExecuteConfirm();

    internal abstract TerminalNode ExecuteDeny();

    internal virtual TerminalNode ExecuteInvalid()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Invalid input. Please input 'confirm' or 'deny'.");

        return TerminalUtils.CreateTerminalNode(
            displayText: builder.ToString(),
            clearPreviousText: false
        );
    }

    internal override TerminalNode? ExecutePrefix(string[] args)
    {
        if (waitingTerminalNode == null)
        {
            return null;
        }

        var confirmResult = IsConfirmed(args);
        if (confirmResult == null)
        {
            return ExecuteInvalid();
        }

        if (confirmResult == true)
        {
            return ExecuteConfirm();
        }

        return ExecuteDeny();
    }

    internal override void ExecutePrefixPostfix(TerminalNode terminalNode)
    {
        waitingTerminalNode = null;
    }

    internal override void ExecuteCorePostfix(TerminalNode terminalNode)
    {
        waitingTerminalNode = terminalNode;
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
