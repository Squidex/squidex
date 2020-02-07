// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public interface IIndexerFactory
    {
        Task<ITextIndexer> CreateAsync(Guid schemaId);

        Task CleanupAsync();
    }
}
