// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using NJsonSchema.Annotations;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Actions
{
    [JsonSchema("ElasticSearch")]
    public sealed class ElasticSearchActionDto : RuleActionDto
    {
        /// <summary>
        /// The host to the elastic search instance.
        /// </summary>
        [Required]
        public Uri Host { get; set; }

        /// <summary>
        /// The name of the index.
        /// </summary>
        [Required]
        public string IndexName { get; set; }

        /// <summary>
        /// The name of the index type.
        /// </summary>
        [Required]
        public string IndexType { get; set; }

        /// <summary>
        /// The optional username for authentication.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The optional password for authentication.
        /// </summary>
        public string Password { get; set; }

        public override RuleAction ToAction()
        {
            return SimpleMapper.Map(this, new ElasticSearchAction());
        }
    }
}
