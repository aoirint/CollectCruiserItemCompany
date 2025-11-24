#nullable enable

using System.Text;
using CollectCruiserItemCompany.Utils;

namespace CollectCruiserItemCompany.Commands;

internal class HelpCommand : Command
{
    internal override bool IsMatch(string[] args)
    {
        if (args.Length == 0)
        {
            return false;
        }

        var command = args[0].ToLower();
        if (command != "collect")
        {
            return false;
        }

        return true;
    }

    internal override TerminalNode Execute(string[] args)
    {
        var builder = new StringBuilder();

        builder.AppendLine($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION}");
        builder.AppendLine();

        builder.AppendLine(">COLLECT SCRAP");
        builder.AppendLine("Collect all scraps on the cruiser into your ship.");
        builder.AppendLine();

        builder.AppendLine(">VIEW COLLECT SCRAP");
        builder.AppendLine("View all scraps on the cruiser that can be collected into your ship.");
        builder.AppendLine();

        builder.AppendLine(">COLLECT TOOL");
        builder.AppendLine("Collect all tools on the cruiser into your ship.");
        builder.AppendLine();

        builder.AppendLine(">VIEW COLLECT TOOL");
        builder.AppendLine("View all tools on the cruiser that can be collected into your ship.");
        builder.AppendLine();

        builder.AppendLine(">COLLECT ALL");
        builder.AppendLine("Collect all items on the cruiser into your ship.");
        builder.AppendLine();

        builder.AppendLine(">VIEW COLLECT ALL");
        builder.AppendLine("View all items on the cruiser that can be collected into your ship.");
        builder.AppendLine();

        return TerminalUtils.CreateTerminalNode(
            displayText: builder.ToString(),
            clearPreviousText: true
        );
    }
}
