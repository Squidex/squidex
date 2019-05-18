// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Extensions.Actions.SendGrid
{
    [RuleAction(
        IconImage = "<svg height='32px' width='32px' viewBox='0 0 64 64' xmlns='http://www.w3.org/2000/svg'><path d='m0 21.25h21.374v21.374h-21.374z' fill='#fff'/><path d='m0 21.25h21.374v21.374h-21.374z' fill='#99e1f4'/><path d='m21.374 42.626h21.25v21.25h-21.25z' fill='#fff'/><path d='m21.374 42.626h21.25v21.25h-21.25z' fill='#99e1f4'/><path d='m0 63.877h21.374v.123h-21.374zm0-21.25h21.374v21.25h-21.374z' fill='#1a82e2'/><path d='m21.374 0h21.25v21.25h-21.25zm21.252 21.374h21.374v21.25h-21.374z' fill='#00b3e3'/><path d='m21.374 42.626h21.25v-21.376h-21.25z' fill='#009dd9'/><g fill='#1a82e2'><path d='m42.626 0h21.374v21.25h-21.374z'/><path d='m42.626 21.25h21.374v.123h-21.374z'/></g></svg>",
        IconColor = "#FFFFFF",
        Display = "Send an email via sendgrid ",
        Description = "Send mail via sendgrid WebAPI.",
        ReadMore = "https://sendgrid.com/")]
    public sealed class SendGridAction : RuleAction
    {
        [Required]
        [Display(Name = "APIKey", Description = "Sendgrid's API Key.")]
        [DataType(DataType.Text)]
        public string APIKey { get; set; }

        [Required]
        [Display(Name = "From Address", Description = "The email sending address.")]
        [DataType(DataType.Text)]
        [Formattable]
        public string MessageFrom { get; set; }

        [Required]
        [Display(Name = "To Address", Description = "The email message will be sent to.")]
        [DataType(DataType.Text)]
        [Formattable]
        public string MessageTo { get; set; }

        [Required]
        [Display(Name = "Subject", Description = "The subject line for this email message.")]
        [DataType(DataType.Text)]
        [Formattable]
        public string MessageSubject { get; set; }

        [Required]
        [Display(Name = "Body", Description = "The message body.")]
        [DataType(DataType.MultilineText)]
        [Formattable]
        public string MessageBody { get; set; }
    }
}
