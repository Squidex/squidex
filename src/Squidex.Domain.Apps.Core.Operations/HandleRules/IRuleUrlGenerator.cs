// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public interface IRuleUrlGenerator
    {
        string GenerateContentUIUrl(NamedId<Guid> appId, NamedId<Guid> schemaId, Guid contentId);
    }
}
