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
using Squidex.Core;

namespace Squidex.Read.Contents.Repositories
{
    public interface IContentRepository
    {
        Task<IReadOnlyList<IContentEntity>> QueryAsync(Guid schemaId, bool nonPublished, string odataQuery, LanguagesConfig languagesConfig);

        Task<long> CountAsync(Guid schemaId, bool nonPublished, string odataQuery, LanguagesConfig languagesConfig);

        Task<IContentEntity> FindContentAsync(Guid schemaId, Guid id);
    }
}
