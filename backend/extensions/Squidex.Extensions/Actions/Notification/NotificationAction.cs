// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.Notification;

[RuleAction(
    Title = "Notification",
    IconImage = "<svg version='1.1' xmlns='http://www.w3.org/2000/svg' width='24' height='24' viewBox='0 0 24 24'><path d='M20.016 15.984v-12h-16.031v14.016l2.016-2.016h14.016zM20.016 2.016c1.078 0 1.969 0.891 1.969 1.969v12c0 1.078-0.891 2.016-1.969 2.016h-14.016l-3.984 3.984v-18c0-1.078 0.891-1.969 1.969-1.969h16.031z'></path></svg>",
    IconColor = "#3389ff",
    Display = "Send a notification",
    Description = "Send an integrated notification to a user.")]
public sealed record NotificationAction : RuleAction
{
    [LocalizedRequired]
    [Display(Name = "User", Description = "The user id or email.")]
    [Editor(RuleFieldEditor.Text)]
    public string User { get; set; }

    [LocalizedRequired]
    [Display(Name = "Title", Description = "The text to send.")]
    [Editor(RuleFieldEditor.TextArea)]
    [Formattable]
    public string Text { get; set; }

    [Display(Name = "Url", Description = "The optional url to attach to the notification.")]
    [Editor(RuleFieldEditor.Text)]
    [Formattable]
    public string Url { get; set; }

    [Display(Name = "Client", Description = "An optional client name.")]
    [Editor(RuleFieldEditor.Text)]
    public string Client { get; set; }
}
