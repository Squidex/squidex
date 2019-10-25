﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public sealed class CommentsLoader : ICommentsLoader
    {
        private readonly IGrainFactory grainFactory;

        public CommentsLoader(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public Task<CommentsResult> GetCommentsAsync(Guid id, long version = EtagVersion.Any)
        {
            var grain = grainFactory.GetGrain<ICommentsGrain>(id);

            return grain.GetCommentsAsync(version);
        }
    }
}
