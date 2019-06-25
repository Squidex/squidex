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

        public Task<StatusInfo> GetInitialStatusAsync(ISchemaEntity schema)
        {
            var result = InfoDraft;

            return Task.FromResult(result);
        }

        public Task<bool> CanMoveToAsync(IContentEntity content, Status next)
        {
            var result = Flow.TryGetValue(content.Status, out var step) && step.Transitions.Any(x => x.Status == next);

            return Task.FromResult(result);
        }

        public Task<bool> CanUpdateAsync(IContentEntity content)
        {
            var result = content.Status != Status.Archived;

            return Task.FromResult(result);
        }

        public Task<StatusInfo> GetInfoAsync(Status status)
        {
            var result = Flow[status].Info;

            return Task.FromResult(result);
        }

        public Task<StatusInfo[]> GetNextsAsync(IContentEntity content)
        {
            var result = Flow.TryGetValue(content.Status, out var step) ? step.Transitions : Array.Empty<StatusInfo>();

            return Task.FromResult(result);
        }

        public Task<StatusInfo[]> GetAllAsync(ISchemaEntity schema)
        {
            return Task.FromResult(All);
        }
    }
}
