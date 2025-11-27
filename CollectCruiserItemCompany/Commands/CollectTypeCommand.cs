#nullable enable

using System.Linq;
using System.Text;
using CollectCruiserItemCompany.Managers;
using CollectCruiserItemCompany.Utils;

namespace CollectCruiserItemCompany.Commands;

internal class CollectTypeCommand : ConfirmableCommand
{
    internal readonly CollectType CollectType;

    internal CollectTypeCommand(CollectType collectType)
    {
        CollectType = collectType;
    }

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

        if (CollectType == CollectType.All && KeywordUtils.IsAllArgument(arg1))
        {
            return true;
        }
        else if (CollectType == CollectType.Scrap && KeywordUtils.IsScrapArgument(arg1))
        {
            return true;
        }
        else if (CollectType == CollectType.Tool && KeywordUtils.IsToolArgument(arg1))
        {
            return true;
        }

        return false;
    }

    internal override ExecuteResult? ExecuteCore(string[] args)
    {
        var isCollectionAllowed = CollectCruiserItemCompany.CollectCruiserItemManager.IsCruiserCollectionAllowed();
        if (!isCollectionAllowed)
        {
            Logger.LogInfo("Cruiser collection is not allowed in the current round state. Cannot proceed with collection request.");

            return new ExecuteResult(
                terminalNode: CreateCollectionRequestNotAllowedNode(),
                nextWaitingCommand: null
            );
        }

        var builder = new StringBuilder();

        if (CollectType == CollectType.All)
        {
            builder.AppendLine("You have requested to collect all items from the cruiser.");
        }
        else if (CollectType == CollectType.Scrap)
        {
            builder.AppendLine("You have requested to collect all scraps from the cruiser.");
        }
        else if (CollectType == CollectType.Tool)
        {
            builder.AppendLine("You have requested to collect all tools from the cruiser.");
        }

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
        var isCollectionAllowed = CollectCruiserItemCompany.CollectCruiserItemManager.IsCruiserCollectionAllowed();
        if (!isCollectionAllowed)
        {
            Logger.LogInfo("Cruiser collection is not allowed in the current round state. Cannot proceed with collection request.");

            return new ExecuteResult(
                terminalNode: CreateCollectionRequestNotAllowedNode(),
                nextWaitingCommand: null
            );
        }

        var collectionNetworkBehaviour = NetworkBehaviourUtils.GetCollectionNetworkBehaviour();
        if (collectionNetworkBehaviour == null)
        {
            Logger.LogError("CollectionNetworkBehaviour is null. Cannot send collect request.");

            return new ExecuteResult(
                terminalNode: CreateCollectionRequestNotAllowedNode(),
                nextWaitingCommand: null
            );
        }

        collectionNetworkBehaviour.CollectRequestServerRpc(
            collectType: CollectType
        );

        var builder = new StringBuilder();

        if (CollectType == CollectType.All)
        {
            builder.AppendLine("Starting collection of all items from the cruiser.");
        }
        else if (CollectType == CollectType.Scrap)
        {
            builder.AppendLine("Starting collection of all scraps from the cruiser.");
        }
        else if (CollectType == CollectType.Tool)
        {
            builder.AppendLine("Starting collection of all tools from the cruiser.");
        }
        else
        {
            Logger.LogError($"Unknown CollectType. CollectType={CollectType}");

            return new ExecuteResult(
                terminalNode: CreateCollectionRequestFailedNode(),
                nextWaitingCommand: null
            );
        }

        builder.AppendLine("");
        builder.AppendLine("If the host player does not allow collection by clients, this request will be ignored.");

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

    internal TerminalNode CreateCollectionRequestNotAllowedNode()
    {
        return TerminalUtils.CreateTerminalNode(
            displayText: "Collection is not allowed currently.",
            clearPreviousText: false
        );
    }

    internal TerminalNode CreateCollectionRequestFailedNode()
    {
        return TerminalUtils.CreateTerminalNode(
            displayText: "Failed to request collection.",
            clearPreviousText: false
        );
    }
}
