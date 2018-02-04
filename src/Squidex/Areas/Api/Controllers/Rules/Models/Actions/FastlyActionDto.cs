// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using NJsonSchema.Annotations;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Actions
{
    [JsonSchema("Fastly")]
    public sealed class FastlyActionDto : RuleActionDto
    {
        /// <summary>
        /// The ID of the fastly service.
        /// </summary>
        [Required]
        public string ServiceId { get; set; }

        /// <summary>
        /// The API key to grant access to Squidex.
        /// </summary>
        [Required]
        public string ApiKey { get; set; }

        public override RuleAction ToAction()
        {
            return SimpleMapper.Map(this, new FastlyAction());
        }
    }
}
