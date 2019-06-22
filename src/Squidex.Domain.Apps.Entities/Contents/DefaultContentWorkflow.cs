// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class DefaultContentWorkflow : IContentWorkflow
    {
        private static readonly Status[] All = { Status.Archived, Status.Draft, Status.Published };

        private static readonly Dictionary<Status, Status[]> Flow = new Dictionary<Status, Status[]>
        {
            [Status.Draft] = new[] { Status.Archived, Status.Published },
            [Status.Archived] = new[] { Status.Draft },
            [Status.Published] = new[] { Status.Draft, Status.Archived }
        };

        public Task<Status> GetInitialStatusAsync(ISchemaEntity schema)
        {
            return Task.FromResult(Status.Draft);
        }

        public Task<bool> CanMoveToAsync(IContentEntity content, Status next)
        {
            return Task.FromResult(Flow.TryGetValue(content.Status, out var state) && state.Contains(next));
        }

        public Task<bool> CanUpdateAsync(IContentEntity content)
        {
            return Task.FromResult(content.Status != Status.Archived);
        }

        public Task<Status[]> GetNextsAsync(IContentEntity content)
        {
            return Task.FromResult(Flow.TryGetValue(content.Status, out var result) ? result : Array.Empty<Status>());
        }

        public Task<Status[]> GetAllAsync(ISchemaEntity schema)
        {
            return Task.FromResult(All);
        }
    }
}
