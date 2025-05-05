// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Management;
using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Flows;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.SignalR;

[FlowStep(
    Title = "Azure SignalR",
    IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'><path d='M.011 16L0 6.248l12-1.63V16zM14 4.328L29.996 2v14H14zM30 18l-.004 14L14 29.75V18zM12 29.495L.01 27.851.009 18H12z'/></svg>",
    IconColor = "#1566BF",
    Display = "Send to Azure SignalR",
    Description = "Send a message to Azure SignalR.",
    ReadMore = "https://azure.microsoft.com/en-en/services/signalr-service/")]
#pragma warning disable CS0618 // Type or member is obsolete
public sealed partial record SignalRFlowStep : FlowStep, IConvertibleToAction
#pragma warning restore CS0618 // Type or member is obsolete
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
    public SignalRActionType Action { get; set; }

    [Display(Name = "Methode Name", Description = "Set the Name of the hub method received by the customer.")]
    [Editor(FlowStepEditor.Text)]
    public string? MethodName { get; set; }

    [Display(Name = "Target (Optional)", Description = "Define target users or groups by id or name. One item per line. Not needed for Broadcast action.")]
    [Editor(FlowStepEditor.TextArea)]
    [Expression]
    public string? Target { get; set; }

    [Display(Name = "Payload (Optional)", Description = "Leave it empty to use the full event as body.")]
    [Editor(FlowStepEditor.TextArea)]
    [Expression(ExpressionFallback.Envelope)]
    public string? Payload { get; set; }

    private static readonly ClientPool<(string ConnectionString, string HubName), ServiceManager> Clients = new (key =>
    {
        var serviceManager = new ServiceManagerBuilder()
            .WithOptions(option =>
            {
                option.ConnectionString = key.ConnectionString;
                option.ServiceTransportType = ServiceTransportType.Transient;
            })
            .BuildServiceManager();

        return serviceManager;
    });

    public override ValueTask ValidateAsync(FlowValidationContext validationContext, AddStepError addError,
        CancellationToken ct)
    {
        if (HubName != null && !HubNameRegex().IsMatch(HubName))
        {
            addError(nameof(HubName), "Hub must be valid azure hub name.");
        }

        if (Action != SignalRActionType.Broadcast && string.IsNullOrWhiteSpace(Target))
        {
            addError(nameof(HubName), "Hub must be valid azure hub name.");
        }

        return default;
    }

    public override async ValueTask<FlowStepResult> ExecuteAsync(FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        if (executionContext.IsSimulation)
        {
            executionContext.LogSkipSimulation();
            return Next();
        }

        var signalR = await Clients.GetClientAsync((ConnectionString, HubName));

        var targets = Target?.Split("\n") ?? [];

        await using (var signalRContext = await signalR.CreateHubContextAsync(HubName, ct))
        {
            var methodeName = !string.IsNullOrWhiteSpace(MethodName) ? MethodName : "push";

            switch (Action)
            {
                case SignalRActionType.Broadcast:
                    await signalRContext.Clients.All.SendAsync(methodeName, Payload, ct);
                    break;
                case SignalRActionType.User:
                    await signalRContext.Clients.Users(targets).SendAsync(methodeName, Payload, ct);
                    break;
                case SignalRActionType.Group:
                    await signalRContext.Clients.Groups(targets).SendAsync(methodeName, Payload, ct);
                    break;
            }
        }

        return Next();
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public RuleAction ToAction()
    {
        return SimpleMapper.Map(this, new SignalRAction());
    }
#pragma warning restore CS0618 // Type or member is obsolete

    [GeneratedRegex("^[a-z][a-z0-9]{2,}(\\-[a-z0-9]+)*$")]
    private static partial Regex HubNameRegex();
}
