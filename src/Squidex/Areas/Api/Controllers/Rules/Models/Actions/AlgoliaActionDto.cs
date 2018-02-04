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
    [JsonSchema("Algolia")]
    public sealed class AlgoliaActionDto : RuleActionDto
    {
        /// <summary>
        /// The application ID.
        /// </summary>
        [Required]
        public string AppId { get; set; }

        /// <summary>
        /// The API key to grant access to Squidex.
        /// </summary>
        [Required]
        public string ApiKey { get; set; }

        /// <summary>
        /// The name of the index.
        /// </summary>
        [Required]
        public string IndexName { get; set; }

        public override RuleAction ToAction()
        {
            return SimpleMapper.Map(this, new AlgoliaAction());
        }
    }
}
