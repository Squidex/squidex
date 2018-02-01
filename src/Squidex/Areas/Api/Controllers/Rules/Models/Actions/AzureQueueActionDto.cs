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
    [JsonSchema("AzureQueue")]
    public class AzureQueueActionDto : RuleActionDto
    {
        /// <summary>
        /// The connection string to the storage account.
        /// </summary>
        [Required]
        public string ConnectionString { get; set; }

        /// <summary>
        /// The queue name.
        /// </summary>
        [Required]
        public string Queue { get; set; }

        public override RuleAction ToAction()
        {
            return SimpleMapper.Map(this, new AzureQueueAction());
        }
    }
}
