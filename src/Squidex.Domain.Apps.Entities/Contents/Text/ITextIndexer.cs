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
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public interface ITextIndexer
    {
        Task DeleteAsync(Guid schemaId, Guid id);

        Task IndexAsync(Guid schemaId, Guid id, NamedContentData data, NamedContentData dataDraft);

        Task<List<Guid>> SearchAsync(string queryText, IAppEntity appEntity, ISchemaEntity schemaEntity, bool useDraft = false);
    }
}
