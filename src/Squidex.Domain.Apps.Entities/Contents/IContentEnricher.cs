// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public interface IContentEnricher
    {
        Task<IContentEntityEnriched> EnrichAsync(IContentEntity content);

        Task<IReadOnlyList<IContentEntityEnriched>> EnrichAsync(IEnumerable<IContentEntity> contents);
    }
}
