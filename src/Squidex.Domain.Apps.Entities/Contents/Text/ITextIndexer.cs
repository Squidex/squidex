// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public interface ITextIndexer
    {
        Task IndexAsync(Guid schemaId, Guid contentId, NamedContentData draftData, NamedContentData data);

        Task<List<Guid>> SearchAsync(string queryText, IAppEntity app, Guid schemaId, Scope scope = Scope.Published);
    }
}
