#nullable enable

using System.Linq;
using System.Text;
using CollectCruiserItemCompany.Utils;

namespace CollectCruiserItemCompany.Commands;

internal class HelpCommand : Command
{
    internal override bool IsMatch(string[] args)
    {
        args = args.Select(arg => arg.ToLower()).ToArray();

        if (args.Length == 0)
        {
            return false;
        }

        var command = args[0];
        if (command != "collect")
        {
            return false;
        }

        return true;
    }

    internal override ExecuteResult? ExecuteCore(string[] args)
    {
        var builder = new StringBuilder();

        builder.AppendLine($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION}");
        builder.AppendLine();

        builder.AppendLine(">COLLECT SCRAP");
        builder.AppendLine("Collect all scraps from the cruiser into your ship.");
        builder.AppendLine();

        builder.AppendLine(">COLLECT TOOL");
        builder.AppendLine("Collect all tools from the cruiser into your ship.");
        builder.AppendLine();

        builder.AppendLine(">COLLECT ALL");
        builder.AppendLine("Collect all items from the cruiser into your ship.");
        builder.AppendLine();

        builder.AppendLine(">VIEW COLLECT [TYPE]");
        builder.AppendLine("View the list of items that would be collected.");
        builder.AppendLine();

        return new ExecuteResult(
            terminalNode: TerminalUtils.CreateTerminalNode(
                displayText: builder.ToString(),
                clearPreviousText: true
            ),
            nextWaitingCommand: null
        );
    }
}
