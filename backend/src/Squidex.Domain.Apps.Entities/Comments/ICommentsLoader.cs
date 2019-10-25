﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public interface ICommentsLoader
    {
        Task<CommentsResult> GetCommentsAsync(Guid id, long version = EtagVersion.Any);
    }
}