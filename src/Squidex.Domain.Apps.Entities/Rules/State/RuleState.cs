// ==========================================================================
//  RuleState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Domain.Apps.Entities.Rules.State
{
    public sealed class RuleState : DomainObjectState<RuleState>, IRuleEntity
    {
        [JsonProperty]
        public Guid AppId { get; set; }

        [JsonProperty]
        public Rule RuleDef { get; set; }

        [JsonProperty]
        public bool IsDeleted { get; set; }
    }
}
