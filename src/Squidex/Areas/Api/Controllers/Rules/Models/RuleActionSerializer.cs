// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Rules.Actions;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    public sealed class RuleActionSerializer : JsonInheritanceConverter
    {
        public RuleActionSerializer()
            : base("actionType", typeof(RuleAction), RuleActionRegistry.Actions)
        {
        }
    }
}
