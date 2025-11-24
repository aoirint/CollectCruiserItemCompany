#nullable enable

using System.Linq;
using System.Text;
using CollectCruiserItemCompany.Utils;

namespace CollectCruiserItemCompany.Commands;

internal class CollectScrapCommand : ConfirmableCommand
{
    internal override bool IsMatch(string[] args)
    {
        args = [.. args.Select(arg => arg.ToLower())];

        if (args.Length < 2)
        {
            return false;
        }

        var command = args[0];
        var arg1 = args[1];
        if (!KeywordUtils.IsCollectCommand(command))
        {
            return false;
        }
        if (!KeywordUtils.IsScrapArgument(arg1))
        {
            return false;
        }

        return true;
    }

    internal override ExecuteResult? ExecuteCore(string[] args)
    {
        var builder = new StringBuilder();

        builder.AppendLine("You have requested to collect all scraps from the cruiser.");
        builder.AppendLine();
        builder.AppendLine("Please CONFIRM or DENY.");

        return new ExecuteResult(
            terminalNode: TerminalUtils.CreateTerminalNode(
                displayText: builder.ToString(),
                clearPreviousText: true
            ),
            nextWaitingCommand: this
        );
    }

    internal override ExecuteResult ExecuteConfirm()
    {
        var builder = new StringBuilder();

        builder.AppendLine("All scraps have been collected from the cruiser.");

        return new ExecuteResult(
            terminalNode: TerminalUtils.CreateTerminalNode(
                displayText: builder.ToString(),
                clearPreviousText: false
            ),
            nextWaitingCommand: null
        );
    }

    internal override ExecuteResult ExecuteDeny()
    {
        var builder = new StringBuilder();

        builder.AppendLine("Cancelled collection.");

        return new ExecuteResult(
            terminalNode: TerminalUtils.CreateTerminalNode(
                displayText: builder.ToString(),
                clearPreviousText: false
            ),
            nextWaitingCommand: null
        );
    }
}
