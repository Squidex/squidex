// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public struct RuleContext
    {
        public NamedId<DomainId> AppId { get; init; }

        public DomainId RuleId { get; init; }

        public Rule Rule { get; init; }

        public bool IncludeSkipped { get; init; }

        public bool IncludeStale { get; init; }
    }
}
