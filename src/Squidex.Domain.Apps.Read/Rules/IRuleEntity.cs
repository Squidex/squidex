// ==========================================================================
//  IRuleEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Domain.Apps.Read.Rules
{
    public interface IRuleEntity :
        IEntity,
        IEntityWithAppRef,
        IEntityWithCreatedBy,
        IEntityWithLastModifiedBy,
        IEntityWithVersion
    {
        Rule RuleDef { get; }
    }
}
