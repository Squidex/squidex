// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Domain.Apps.Rules.Action.Fastly
{
    public sealed class FastlyAction : RuleAction
    {
        [Required]
        [Display(Name = "Api Key", Description = "The API key to grant access to Squidex.")]
        public string ApiKey { get; set; }

        [Required]
        [Display(Name = "Service Id", Description = "The ID of the fastly service.")]
        public string ServiceId { get; set; }
    }
}
