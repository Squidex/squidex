// ==========================================================================
//  IContentRepoitory.cs
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
    public interface IContentRepoitory
    {
        Task<List<IContentEntity>> QueryAsync();

        Task<IContentEntity> FindContentAsync(Guid id);
    }
}
