﻿using System.Collections.Generic;
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
        [DataType(DataType.Text)]
        [Formattable]
        public string ConnectionString { get; set; }

        [LocalizedRequired]
        [Display(Name = "Hub Name", Description = "The name of the hub.")]
        [DataType(DataType.Text)]
        [Formattable]
        public string HubName { get; set; }

        [LocalizedRequired]
        [Display(Name = "Action", Description = "BROADCAST -> send to all User, USER => send to one user, USERS => send to list of users, GROUP => send to group, GROUPS => send to list of groups.")]
        [DataType(DataType.Text)]
        public ActionTypeEnum ActionType { get; set; }

        [Display(Name = "Methode Name", Description = "Set the Name of the hub method received by the customer, default value 'push.")]
        [DataType(DataType.Text)]
        public string MethodName { get; set; }

        [Display(Name = "User", Description = "Set for notity one user by Id (command 'user'), one id by line for notity multi user (command 'users').")]
        [DataType(DataType.MultilineText)]
        [Formattable]
        public string User { get; set; }

        [Display(Name = "Group", Description = "Set for notity one group by Name (Command 'group'), one id by line for notity multi groups (command 'groups').")]
        [DataType(DataType.MultilineText)]
        [Formattable]
        public string Group { get; set; }

        [Display(Name = "Payload (Optional)", Description = "Leave it empty to use the full event as body.")]
        [DataType(DataType.MultilineText)]
        [Formattable]
        public string Payload { get; set; }

        protected override IEnumerable<ValidationError> CustomValidate()
        {
            if (!string.IsNullOrWhiteSpace(HubName) && !Regex.IsMatch(HubName, "^[a-z][a-z0-9]{2,}(\\-[a-z0-9]+)*$"))
            {
                yield return new ValidationError("Hub must be valid azure hub name.", nameof(HubName));
            }

            if (ActionType == ActionTypeEnum.USER && (string.IsNullOrWhiteSpace(User) || User.IndexOf("\n", System.StringComparison.OrdinalIgnoreCase) >= 0))
            {
                yield return new ValidationError("Group must be specified and unique with 'USER' Action.", nameof(HubName));
            }

            if (ActionType == ActionTypeEnum.USERS && (string.IsNullOrWhiteSpace(User) || User.IndexOf("\n", System.StringComparison.OrdinalIgnoreCase) < 0))
            {
                yield return new ValidationError("User must be specified and multiple with 'USERS' Action.", nameof(HubName));
            }

            if (ActionType == ActionTypeEnum.GROUP && (string.IsNullOrWhiteSpace(Group) || Group.IndexOf("\n", System.StringComparison.OrdinalIgnoreCase) >= 0))
            {
                yield return new ValidationError("Group must be specified and unique with 'GROUP' Action.", nameof(HubName));
            }

            if (ActionType == ActionTypeEnum.GROUPS && (string.IsNullOrWhiteSpace(Group) || Group.IndexOf("\n", System.StringComparison.OrdinalIgnoreCase) < 0))
            {
                yield return new ValidationError("Group must be specified and multiple with 'GROUPS' Action.", nameof(HubName));
            }
        }
    }

    public enum ActionTypeEnum
    {
        BROADCAST,
        USER,
        USERS,
        GROUP,
        GROUPS
    }
}
