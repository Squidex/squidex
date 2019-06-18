// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class DefaultContentWorkflow : IContentWorkflow
    {
        private static readonly Status2 Draft = new Status2("Draft");
        private static readonly Status2 Archived = new Status2("Archived");
        private static readonly Status2 Published = new Status2("Published");

        private static readonly Dictionary<Status2, Status2[]> Flow = new Dictionary<Status2, Status2[]>
        {
            [Draft] = new[] { Published, Archived },
            [Archived] = new[] { Draft },
            [Published] = new[] { Draft, Archived }
        };

        public Task<Status2> GetInitialStatusAsync(ISchemaEntity schema)
        {
            return Task.FromResult(Draft);
        }

        public Task<bool> IsValidNextStatus(IContentEntity content, Status2 next)
        {
            return TaskHelper.True;
        }

        public Task<bool> CanUpdateAsync(IContentEntity content)
        {
            return TaskHelper.True;
        }

        public Task<Status2[]> GetNextsAsync(IContentEntity content)
        {
            Status2 statusToCheck;
            switch (content.Status)
            {
                case Status.Draft:
                    statusToCheck = Draft;
                    break;
                case Status.Archived:
                    statusToCheck = Archived;
                    break;
                case Status.Published:
                    statusToCheck = Published;
                    break;
                default:
                {
                    statusToCheck = Draft;
                    break;
                }
            }

            return Task.FromResult(Flow.TryGetValue(statusToCheck, out var result) ? result : Array.Empty<Status2>());
        }

        public Task<Status2[]> GetAllAsync(ISchemaEntity schema)
        {
            return Task.FromResult(new[] { Draft, Archived, Published } );
        }
    }
}
