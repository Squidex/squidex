// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Extensions.Actions;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    public sealed class RuleActionSerializer : JsonInheritanceConverter
    {
        public RuleActionSerializer()
            : base("actionType", typeof(RuleAction), RuleElementRegistry.Actions.ToDictionary(x => x.Key, x => x.Value.Type))
        {
        }
    }
}
