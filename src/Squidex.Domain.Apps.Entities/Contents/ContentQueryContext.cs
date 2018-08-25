// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentQueryContext : Cloneable<ContentQueryContext>
    {
        public string SchemaIdOrName { get; private set; }

        public QueryContext Base { get; private set; }

        public ContentQueryContext(QueryContext @base)
        {
            Guard.NotNull(@base, nameof(@base));

            Base = @base;
        }

        public ContentQueryContext WithSchemaName(string name)
        {
            return Clone(c => c.SchemaIdOrName = name);
        }

        public ContentQueryContext WithArchived(bool archived)
        {
            return Clone(c => c.Base = c.Base.WithArchived(archived));
        }

        public ContentQueryContext WithFlatten(bool flatten)
        {
            return Clone(c => c.Base = c.Base.WithFlatten(flatten));
        }

        public ContentQueryContext WithUnpublished(bool unpublished)
        {
            return Clone(c => c.Base = c.Base.WithUnpublished(unpublished));
        }

        public ContentQueryContext WithSchemaId(Guid id)
        {
            return Clone(c => c.SchemaIdOrName = id.ToString());
        }
    }
}
