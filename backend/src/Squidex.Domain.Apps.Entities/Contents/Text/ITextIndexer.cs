// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public interface ITextIndexer
    {
        Task<List<Guid>?> SearchAsync(string? queryText, IAppEntity app, Guid schemaId, SearchScope scope = SearchScope.Published);

        Task ExecuteAsync(NamedId<Guid> appId, NamedId<Guid> schemaId, params IndexCommand[] commands);
    }
}
