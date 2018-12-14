// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Extensions.Actions;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    public sealed class RuleActionConverter : MyJsonInheritanceConverter<RuleAction>
    {
        private static readonly Dictionary<string, Type> Mapping = RuleElementRegistry.Actions.ToDictionary(x => x.Key, x => x.Value.Type);

        public RuleActionConverter()
            : base("actionType", Mapping)
        {
        }
    }
}
