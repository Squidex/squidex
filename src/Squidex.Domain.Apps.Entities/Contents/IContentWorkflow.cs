// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public interface IContentWorkflow
    {
        Task<Status> GetInitialStatusAsync(ISchemaEntity schema);

        Task<bool> IsValidNextStatus(IContentEntity content, Status next);

        Task<bool> CanUpdateAsync(IContentEntity content);

        Task<Status[]> GetNextsAsync(IContentEntity content);

        Task<Status[]> GetAllAsync(ISchemaEntity schema);
    }
}
