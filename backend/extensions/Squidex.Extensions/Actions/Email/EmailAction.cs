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

namespace Squidex.Extensions.Actions.Email;

[RuleAction(
    Title = "Email",
    IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'><path d='M28 5h-24c-2.209 0-4 1.792-4 4v13c0 2.209 1.791 4 4 4h24c2.209 0 4-1.791 4-4v-13c0-2.208-1.791-4-4-4zM2 10.25l6.999 5.25-6.999 5.25v-10.5zM30 22c0 1.104-0.898 2-2 2h-24c-1.103 0-2-0.896-2-2l7.832-5.875 4.368 3.277c0.533 0.398 1.166 0.6 1.8 0.6 0.633 0 1.266-0.201 1.799-0.6l4.369-3.277 7.832 5.875zM30 20.75l-7-5.25 7-5.25v10.5zM17.199 18.602c-0.349 0.262-0.763 0.4-1.199 0.4s-0.851-0.139-1.2-0.4l-12.8-9.602c0-1.103 0.897-2 2-2h24c1.102 0 2 0.897 2 2l-12.801 9.602z'/></svg>",
    IconColor = "#333300",
    Display = "Send an email",
    Description = "Send an email with a custom SMTP server.",
    ReadMore = "https://en.wikipedia.org/wiki/Email")]
public sealed record EmailAction : RuleAction
{
    [LocalizedRequired]
    [Display(Name = "Server Host", Description = "The IP address or host to the SMTP server.")]
    [Editor(RuleFieldEditor.Text)]
    public string ServerHost { get; set; }

    [LocalizedRequired]
    [Display(Name = "Server Port", Description = "The port to the SMTP server.")]
    [Editor(RuleFieldEditor.Text)]
    public int ServerPort { get; set; }

    [LocalizedRequired]
    [Display(Name = "Username", Description = "The username for the SMTP server.")]
    [Editor(RuleFieldEditor.Text)]
    [Formattable]
    public string ServerUsername { get; set; }

    [LocalizedRequired]
    [Display(Name = "Password", Description = "The password for the SMTP server.")]
    [Editor(RuleFieldEditor.Password)]
    public string ServerPassword { get; set; }

    [LocalizedRequired]
    [Display(Name = "From Address", Description = "The email sending address.")]
    [Editor(RuleFieldEditor.Text)]
    [Formattable]
    public string MessageFrom { get; set; }

    [LocalizedRequired]
    [Display(Name = "To Address", Description = "The email message will be sent to.")]
    [Editor(RuleFieldEditor.Text)]
    [Formattable]
    public string MessageTo { get; set; }

    [LocalizedRequired]
    [Display(Name = "Subject", Description = "The subject line for this email message.")]
    [Editor(RuleFieldEditor.Text)]
    [Formattable]
    public string MessageSubject { get; set; }

    [LocalizedRequired]
    [Display(Name = "Body", Description = "The message body.")]
    [Editor(RuleFieldEditor.TextArea)]
    [Formattable]
    public string MessageBody { get; set; }
}
