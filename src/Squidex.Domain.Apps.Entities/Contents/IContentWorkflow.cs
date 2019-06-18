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
        Task<Status2> GetInitialStatusAsync(ISchemaEntity schema);

        Task<bool> IsValidNextStatus(IContentEntity content, Status2 next);

        Task<bool> CanUpdateAsync(IContentEntity content);

        Task<Status2[]> GetNextsAsync(IContentEntity content);

        Task<Status2[]> GetAllAsync(ISchemaEntity schema);
    }
}
