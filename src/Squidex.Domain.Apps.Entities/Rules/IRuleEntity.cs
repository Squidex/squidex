﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Domain.Apps.Entities.Rules
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
