﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public interface IEnrichedContentEntity : IContentEntity
    {
        bool CanUpdate { get; }

        string StatusColor { get; }

        string SchemaName { get; }

        string SchemaDisplayName { get; }

        RootField[]? ReferenceFields { get; }

        StatusInfo[]? Nexts { get; }

        NamedContentData? ReferenceData { get; }
    }
}
