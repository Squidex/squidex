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
        Description = "Send an message to azure SignalR.",
        ReadMore = "https://azure.microsoft.com/fr-fr/services/signalr-service/")]
    public sealed record SignalRAction : RuleAction
    {
        [LocalizedRequired]
        [Display(Name = "Connection", Description = "The connection string to the Signal R Azure.")]
        [Editor(RuleFieldEditor.Text)]
        [Formattable]
        public string ConnectionString { get; set; }

        [LocalizedRequired]
        [Display(Name = "Hub Name", Description = "The name of the hub.")]
        [Editor(RuleFieldEditor.Text)]
        [Formattable]
        public string HubName { get; set; }

        [LocalizedRequired]
        [Display(Name = "Action", Description = "Broadcast = send to all User, User = send to specific user(s) specified in the 'Target' field, Group = send to specific group(s) specified in the 'Target' field")]
        public ActionTypeEnum Action { get; set; }

        [Display(Name = "Methode Name", Description = "Set the Name of the hub method received by the customer, default value 'push.")]
        [Editor(RuleFieldEditor.Text)]
        public string MethodName { get; set; }

        [Display(Name = "Target (Optional)", Description = "Defines a user, group or target list by an id or name. For a list, define one value per line. Not necessary with the 'Broadcast' action")]
        [Editor(RuleFieldEditor.TextArea)]
        [Formattable]
        public string Target { get; set; }

        [Display(Name = "Payload (Optional)", Description = "Leave it empty to use the full event as body.")]
        [Editor(RuleFieldEditor.TextArea)]
        [Formattable]
        public string Payload { get; set; }

        protected override IEnumerable<ValidationError> CustomValidate()
        {
            if (!string.IsNullOrWhiteSpace(HubName) && !Regex.IsMatch(HubName, "^[a-z][a-z0-9]{2,}(\\-[a-z0-9]+)*$"))
            {
                yield return new ValidationError("Hub must be valid azure hub name.", nameof(HubName));
            }

            if ((Action == ActionTypeEnum.User || Action == ActionTypeEnum.Group) && string.IsNullOrWhiteSpace(Target))
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
