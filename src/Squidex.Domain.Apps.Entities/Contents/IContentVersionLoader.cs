// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public interface IContentVersionLoader
    {
        Task<IContentEntity> LoadAsync(Guid id, long version);
    }
}