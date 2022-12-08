// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules;

public interface IRuleEntity :
    IEntity,
    IEntityWithCreatedBy,
    IEntityWithLastModifiedBy,
    IEntityWithVersion
{
    NamedId<DomainId> AppId { get; set; }

    Rule RuleDef { get; }

    bool IsDeleted { get; }
}
