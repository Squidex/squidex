// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;

namespace Squidex.Domain.Apps.Entities.Apps.Templates.Builders
{
    public class TagsFieldBuilder : FieldBuilder
    {
        public TagsFieldBuilder(UpsertSchemaField field, CreateSchema schema)
            : base(field, schema)
        {
        }

        public TagsFieldBuilder WithAllowedValues(params string[] values)
        {
            Properties<TagsFieldProperties>().AllowedValues = new ReadOnlyCollection<string>(values);

            return this;
        }
    }
}
