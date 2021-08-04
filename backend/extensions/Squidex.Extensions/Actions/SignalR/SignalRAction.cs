// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.SignalR
{
    [RuleAction(
        Title = "Azure SignalR",
        IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'><path d='M.011 16L0 6.248l12-1.63V16zM14 4.328L29.996 2v14H14zM30 18l-.004 14L14 29.75V18zM12 29.495L.01 27.851.009 18H12z'/></svg>",
        IconColor = "#1566BF",
        Display = "Send to Azure SignalR",
        Description = "Send a message to Azure SignalR.",
        ReadMore = "https://azure.microsoft.com/fr-fr/services/signalr-service/")]
    public sealed record SignalRAction : RuleAction
    {
        [LocalizedRequired]
        [Display(Name = "Connection", Description = "The connection string to the Azure SignalR.")]
        [Editor(RuleFieldEditor.Text)]
        [Formattable]
        public string ConnectionString { get; set; }

        [LocalizedRequired]
        [Display(Name = "Hub Name", Description = "The name of the hub.")]
        [Editor(RuleFieldEditor.Text)]
        [Formattable]
        public string HubName { get; set; }

        [LocalizedRequired]
        [Display(Name = "Action", Description = "* Broadcast = send to all users.\n * User = send to all target users(s).\n * Group = send to all target group(s).")]
        public ActionTypeEnum Action { get; set; }

        [Display(Name = "Methode Name", Description = "Set the Name of the hub method received by the customer.")]
        [Editor(RuleFieldEditor.Text)]
        public string MethodName { get; set; }

        [Display(Name = "Target (Optional)", Description = "Define target users or groups by id or name. One item per line. Not needed for Broadcast action.")]
        [Editor(RuleFieldEditor.TextArea)]
        [Formattable]
        public string Target { get; set; }

        [Display(Name = "Payload (Optional)", Description = "Leave it empty to use the full event as body.")]
        [Editor(RuleFieldEditor.TextArea)]
        [Formattable]
        public string Payload { get; set; }

        protected override IEnumerable<ValidationError> CustomValidate()
        {
            if (HubName != null && !Regex.IsMatch(HubName, "^[a-z][a-z0-9]{2,}(\\-[a-z0-9]+)*$"))
            {
                yield return new ValidationError("Hub must be valid azure hub name.", nameof(HubName));
            }

            if (Action != ActionTypeEnum.Broadcast && string.IsNullOrWhiteSpace(Target))
            {
                yield return new ValidationError("Target must be specified with 'User' or 'Group' Action.", nameof(HubName));
            }
        }
    }

    public enum ActionTypeEnum
    {
        Broadcast,
        User,
        Group
    }
}
