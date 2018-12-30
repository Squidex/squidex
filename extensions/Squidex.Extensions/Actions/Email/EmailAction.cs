// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Extensions.Actions.Email
{
    [RuleActionHandler(typeof(EmailActionHandler))]
    [RuleAction(
        IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'><path d='M28 5h-24c-2.209 0-4 1.792-4 4v13c0 2.209 1.791 4 4 4h24c2.209 0 4-1.791 4-4v-13c0-2.208-1.791-4-4-4zM2 10.25l6.999 5.25-6.999 5.25v-10.5zM30 22c0 1.104-0.898 2-2 2h-24c-1.103 0-2-0.896-2-2l7.832-5.875 4.368 3.277c0.533 0.398 1.166 0.6 1.8 0.6 0.633 0 1.266-0.201 1.799-0.6l4.369-3.277 7.832 5.875zM30 20.75l-7-5.25 7-5.25v10.5zM17.199 18.602c-0.349 0.262-0.763 0.4-1.199 0.4s-0.851-0.139-1.2-0.4l-12.8-9.602c0-1.103 0.897-2 2-2h24c1.102 0 2 0.897 2 2l-12.801 9.602z'/></svg>",
        IconColor = "#333300",
        Display = "Send an email",
        Description = "Send an email",
        ReadMore = "https://en.wikipedia.org/wiki/Email")]
    public class EmailAction : RuleAction
    {
        [Required]
        [Display(Name = "Host", Description = "The Name or IP address of the host used for SMTP transactions.")]
        public string Host { get; set; }

        [Required]
        [Display(Name = "Port", Description = "The port to be used on host.")]
        public int Port { get; set; }

        [Required]
        [Display(Name = "EnableSsl", Description = "Specify whether the smtp client uses Secure Sockets Layer (SSL) to encrypt the connection.")]
        public bool EnableSsl { get; set; }

        [Required]
        [Display(Name = "Username", Description = "The username used to authenticate the sender.")]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Password", Description = "The password used to authenticate the sender.")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "From", Description = "The email is sent from?")]
        public string From { get; set; }

        [Required]
        [Display(Name = "To", Description = "The email will be sent to?")]
        public string To { get; set; }

        [Required]
        [Display(Name = "Subject", Description = "The subject line for this e-mail message.")]
        public string Subject { get; set; }

        [Required]
        [Display(Name = "Body", Description = "The message body.")]
        public string Body { get; set; }
    }
}
