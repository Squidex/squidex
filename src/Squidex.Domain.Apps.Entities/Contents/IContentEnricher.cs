// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public interface IContentEnricher
    {
        Task<IEnrichedContentEntity> EnrichAsync(IContentEntity content, ClaimsPrincipal user);

        Task<IReadOnlyList<IEnrichedContentEntity>> EnrichAsync(IEnumerable<IContentEntity> contents, ClaimsPrincipal user);
    }
}
