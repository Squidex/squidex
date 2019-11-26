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
    public interface IFullTextRepository
    {
        Task DeleteAsync(Guid id);

        Task CopyAsync(Guid id, bool fromDraft);

        Task IndexAsync(Guid id, TextContent content, bool draftOnly);
    }
}
