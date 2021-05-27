// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Entities.Apps.Templates.Builders
{
    public class TagsFieldBuilder : FieldBuilder<TagsFieldBuilder>
    {
        public TagsFieldBuilder(UpsertSchemaField field, CreateSchema schema)
            : base(field, schema)
        {
        }

        public TagsFieldBuilder WithAllowedValues(params string[] values)
        {
            Properties<TagsFieldProperties>(p => p with
            {
                AllowedValues = ImmutableList.Create(values)
            });

            return this;
        }
    }
}
