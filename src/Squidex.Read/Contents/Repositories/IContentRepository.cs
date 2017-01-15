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

namespace Squidex.Read.Contents.Repositories
{
    public interface IContentRepository
    {
        Task<List<IContentEntity>> QueryAsync(Guid schemaId, bool nonPublished, int? take, int? skip, string query);

        Task<long> CountAsync(Guid schemaId, bool nonPublished, string query);

        Task<IContentEntity> FindContentAsync(Guid schemaId, Guid id);
    }
}
