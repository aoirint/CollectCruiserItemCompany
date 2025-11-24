#nullable enable

using System.Linq;
using System.Text;
using CollectCruiserItemCompany.Utils;

namespace CollectCruiserItemCompany.Commands;

internal class CollectScrapCommand : ConfirmableCommand
{
    internal override bool IsMatch(string[] args)
    {
        args = args.Select(arg => arg.ToLower()).ToArray();

        if (args.Length < 2)
        {
            return false;
        }

        var command = args[0];
        var arg1 = args[1];
        if (command != "collect")
        {
            return false;
        }
        if (arg1 != "scrap")
        {
            return false;
        }

        return true;
    }

    internal override TerminalNode ExecuteCore(string[] args)
    {
        var builder = new StringBuilder();

        builder.AppendLine("Are you sure you want to collect all scraps from the cruiser?");
        builder.AppendLine("Type 'confirm' to proceed or 'deny' to cancel.");

        return TerminalUtils.CreateTerminalNode(
            displayText: builder.ToString(),
            clearPreviousText: true
        );
    }

    internal override TerminalNode ExecuteConfirm()
    {
        var builder = new StringBuilder();

        builder.AppendLine("All scraps have been collected from the cruiser.");

        return TerminalUtils.CreateTerminalNode(
            displayText: builder.ToString(),
            clearPreviousText: false
        );
    }

    internal override TerminalNode ExecuteDeny()
    {
        var builder = new StringBuilder();

        builder.AppendLine("Cancelled collection.");

        return TerminalUtils.CreateTerminalNode(
            displayText: builder.ToString(),
            clearPreviousText: false
        );
    }
}
