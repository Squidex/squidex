// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public interface ITextIndex
    {
        Task<List<DomainId>?> SearchAsync(IAppEntity app, TextQuery query, SearchScope scope);

        Task<List<DomainId>?> SearchAsync(IAppEntity app, GeoQuery query, SearchScope scope);

        Task ClearAsync();

        Task ExecuteAsync(params IndexCommand[] commands);
    }
}
