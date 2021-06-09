// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;
using Squidex.Domain.Apps.Entities.Apps;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public interface ISchemasHash
    {
        Task<(Instant Create, string Hash)> GetCurrentHashAsync(IAppEntity app);

        ValueTask<string> ComputeHashAsync(IAppEntity app, IEnumerable<ISchemaEntity> schemas);
    }
}
