﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public sealed class FilterTagTransformer : TransformVisitor<ClrValue>
    {
        private readonly ITagService tagService;
        private readonly ISchemaEntity schema;
        private readonly Guid appId;

        private FilterTagTransformer(Guid appId, ISchemaEntity schema, ITagService tagService)
        {
            this.appId = appId;
            this.schema = schema;
            this.tagService = tagService;
        }

        public static FilterNode<ClrValue> Transform(FilterNode<ClrValue> nodeIn, Guid appId, ISchemaEntity schema, ITagService tagService)
        {
            Guard.NotNull(tagService, nameof(tagService));
            Guard.NotNull(schema, nameof(schema));

            return nodeIn.Accept(new FilterTagTransformer(appId, schema, tagService));
        }

        public override FilterNode<ClrValue> Visit(CompareFilter<ClrValue> nodeIn)
        {
            if (nodeIn.Value.Value is string stringValue && IsDataPath(nodeIn.Path) && IsTagField(nodeIn.Path))
            {
                var tagNames = Task.Run(() => tagService.GetTagIdsAsync(appId, TagGroups.Schemas(schema.Id), HashSet.Of(stringValue))).Result;

                if (tagNames.TryGetValue(stringValue, out var normalized))
                {
                    return new CompareFilter<ClrValue>(nodeIn.Path, nodeIn.Operator, normalized);
                }
            }

            return nodeIn;
        }

        private static bool IsDataPath(IReadOnlyList<string> path)
        {
            return path.Count == 3 && string.Equals(path[0], nameof(IContentEntity.Data), StringComparison.OrdinalIgnoreCase);
        }

        private bool IsTagField(IReadOnlyList<string> path)
        {
            return schema.SchemaDef.FieldsByName.TryGetValue(path[1], out var field) &&
                field is IField<TagsFieldProperties> fieldTags &&
                fieldTags.Properties.Normalization == TagsFieldNormalization.Schema;
        }
    }
}
