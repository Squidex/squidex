// ==========================================================================
//  IContentRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Read.Contents.Repositories
{
    public interface IContentRepository
    {
        Task<List<IContentEntity>> QueryAsync(Guid schemaId, bool nonPublished, string odataQuery, HashSet<Language> languages);

        Task<long> CountAsync(Guid schemaId, bool nonPublished, string odataQuery, HashSet<Language> languages);

        Task<IContentEntity> FindContentAsync(Guid schemaId, Guid id);
    }
}
