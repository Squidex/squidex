// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class DefaultContentWorkflow : IContentWorkflow
    {
        private static readonly StatusInfo InfoArchived = new StatusInfo(Status.Archived, StatusColors.Archived);
        private static readonly StatusInfo InfoDraft = new StatusInfo(Status.Draft, StatusColors.Draft);
        private static readonly StatusInfo InfoPublished = new StatusInfo(Status.Published, StatusColors.Published);

        private static readonly StatusInfo[] All =
        {
            InfoArchived,
            InfoDraft,
            InfoPublished
        };

        private static readonly Dictionary<Status, (StatusInfo Info, StatusInfo[] Transitions)> Flow =
            new Dictionary<Status, (StatusInfo Info, StatusInfo[] Transitions)>
            {
                [Status.Archived] = (InfoArchived, new[]
                {
                    InfoDraft
                }),
                [Status.Draft] = (InfoDraft, new[]
                {
                    InfoArchived,
                    InfoPublished
                }),
                [Status.Published] = (InfoPublished, new[]
                {
                    InfoDraft,
                    InfoArchived
                })
            };

        public Task<Status> GetInitialStatusAsync(ISchemaEntity schema)
        {
            return Task.FromResult(Status.Draft);
        }

        public Task<bool> CanPublishOnCreateAsync(ISchemaEntity schema, NamedContentData data, ClaimsPrincipal user)
        {
            return TaskHelper.True;
        }

        public Task<bool> CanMoveToAsync(IContentInfo content, Status status, Status next, ClaimsPrincipal user)
        {
            var result = Flow.TryGetValue(status, out var step) && step.Transitions.Any(x => x.Status == next);

            return Task.FromResult(result);
        }

        public Task<bool> CanUpdateAsync(IContentInfo content, Status status, ClaimsPrincipal user)
        {
            var result = status != Status.Archived;

            return Task.FromResult(result);
        }

        public Task<StatusInfo> GetInfoAsync(IContentInfo content, Status status)
        {
            var result = Flow[status].Info;

            return Task.FromResult(result);
        }

        public Task<StatusInfo[]> GetNextAsync(IContentInfo content, Status status, ClaimsPrincipal user)
        {
            var result = Flow.TryGetValue(status, out var step) ? step.Transitions : Array.Empty<StatusInfo>();

            return Task.FromResult(result);
        }

        public Task<StatusInfo[]> GetAllAsync(ISchemaEntity schema)
        {
            return Task.FromResult(All);
        }
    }
}
