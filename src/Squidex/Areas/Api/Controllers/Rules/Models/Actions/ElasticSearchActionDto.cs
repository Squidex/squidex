// ==========================================================================
//  ElasticSearchActionDto.cs
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
    [JsonSchema("ElasticSearchAction")]
    public class ElasticSearchActionDto : RuleActionDto
    {
        [Required]
        public string TypeNameForSchema { get; set; }

        [Required]
        public string IndexName { get; set; }

        public override RuleAction ToAction()
        {
            return SimpleMapper.Map(this, new ElasticSearchAction());
        }
    }
}