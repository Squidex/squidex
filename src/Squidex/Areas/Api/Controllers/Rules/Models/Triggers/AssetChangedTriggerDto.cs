// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema.Annotations;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Triggers
{
    [JsonSchema("AssetChanged")]
    public sealed class AssetChangedTriggerDto : RuleTriggerDto
    {
        /// <summary>
        /// Determines whether to handle the event when an asset is created.
        /// </summary>
        public bool SendCreate { get; set; }

        /// <summary>
        /// Determines whether to handle the event when an asset is updated.
        /// </summary>
        public bool SendUpdate { get; set; }

        /// <summary>
        /// Determines whether to handle the event when an asset is renamed.
        /// </summary>
        public bool SendRename { get; set; }

        /// <summary>
        /// Determines whether to handle the event when an asset is deleted.
        /// </summary>
        public bool SendDelete { get; set; }

        public override RuleTrigger ToTrigger()
        {
            return SimpleMapper.Map(this, new AssetChangedTrigger());
        }
    }
}
