// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Flows;
using Squidex.Infrastructure.Validation;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Extensions.Actions.SignalR;

[FlowStep(
    Title = "Azure SignalR",
    IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'><path d='M.011 16L0 6.248l12-1.63V16zM14 4.328L29.996 2v14H14zM30 18l-.004 14L14 29.75V18zM12 29.495L.01 27.851.009 18H12z'/></svg>",
    IconColor = "#1566BF",
    Display = "Send to Azure SignalR",
    Description = "Send a message to Azure SignalR.",
    ReadMore = "https://azure.microsoft.com/fr-fr/services/signalr-service/")]
public sealed partial class SignalRStep : RuleFlowStep<EnrichedEvent>
{
    [LocalizedRequired]
    [Display(Name = "Connection", Description = "The connection string to the Azure SignalR.")]
    [Editor(FlowStepEditor.Text)]
    [Expression]
    public string ConnectionString { get; set; }

    [LocalizedRequired]
    [Display(Name = "Hub Name", Description = "The name of the hub.")]
    [Editor(FlowStepEditor.Text)]
    [Expression]
    public string HubName { get; set; }

    [LocalizedRequired]
    [Display(Name = "Action", Description = "* Broadcast = send to all users.\n * User = send to all target users(s).\n * Group = send to all target group(s).")]
    public ActionTypeEnum Action { get; set; }

    [Display(Name = "Methode Name", Description = "Set the Name of the hub method received by the customer.")]
    [Editor(FlowStepEditor.Text)]
    public string? MethodName { get; set; }

    [Display(Name = "Target (Optional)", Description = "Define target users or groups by id or name. One item per line. Not needed for Broadcast action.")]
    [Editor(FlowStepEditor.TextArea)]
    [Expression]
    public string? Target { get; set; }

    [Display(Name = "Payload (Optional)", Description = "Leave it empty to use the full event as body.")]
    [Editor(FlowStepEditor.TextArea)]
    [Expression]
    public string? Payload { get; set; }

    public override ValueTask ValidateAsync(FlowValidationContext validationContext, AddError addError,
        CancellationToken ct)
    {
        if (HubName != null && !HubNameRegex().IsMatch(HubName))
        {
            addError("Hub must be valid azure hub name.", ValidationErrorType.InvalidProperty, "hubName");
        }

        if (Action != ActionTypeEnum.Broadcast && string.IsNullOrWhiteSpace(Target))
        {
            addError("Target must be specified with 'User' or 'Group' Action.", ValidationErrorType.InvalidProperty, "target");
        }

        return default;
    }

    protected override async ValueTask<FlowStepResult> ExecuteAsync(RuleFlowContext context, EnrichedEvent @event, FlowExecutionContext executionContext,
    CancellationToken ct)
    {
        var signalRPool = executionContext.Resolve<SignalRClientPool>();
        var signalRClient = await signalRPool.GetClientAsync((ConnectionString, HubName));

        await using var signalRContext = await signalRClient.CreateHubContextAsync(HubName, ct);

        var methodName = MethodName;
        if (string.IsNullOrWhiteSpace(methodName))
        {
            methodName = "push";
        }

        var targets = Target?.Split("\n") ?? [];

        switch (Action)
        {
            case ActionTypeEnum.Broadcast:
                await signalRContext.Clients.All.SendAsync(methodName, Payload, ct);
                break;
            case ActionTypeEnum.User:
                await signalRContext.Clients.Users(targets).SendAsync(methodName, Payload, ct);
                break;
            case ActionTypeEnum.Group:
                await signalRContext.Clients.Groups(targets).SendAsync(methodName, Payload, ct);
                break;
        }

        return FlowStepResult.Next();
    }

    [GeneratedRegex("^[a-z][a-z0-9]{2,}(\\-[a-z0-9]+)*$")]
    private static partial Regex HubNameRegex();
}

public enum ActionTypeEnum
{
    Broadcast,
    User,
    Group
}
