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
    public class StringFieldBuilder : FieldBuilder<StringFieldBuilder>
    {
        public StringFieldBuilder(UpsertSchemaFieldBase field, CreateSchema schema)
            : base(field, schema)
        {
        }

        public StringFieldBuilder AsTextArea()
        {
            Properties<StringFieldProperties>(p => p with
            {
                EditorUrl = null,
                Editor = StringFieldEditor.TextArea
            });

            return this;
        }

        public StringFieldBuilder AsRichText()
        {
            Properties<StringFieldProperties>(p => p with
            {
                EditorUrl = null,
                Editor = StringFieldEditor.RichText
            });

            return this;
        }

        public StringFieldBuilder AsDropDown(params string[] values)
        {
            Properties<StringFieldProperties>(p => p with
            {
                AllowedValues = ImmutableList.Create(values),
                EditorUrl = null,
                Editor = StringFieldEditor.Dropdown
            });

            return this;
        }

        public StringFieldBuilder Unique()
        {
            Properties<StringFieldProperties>(p => p with
            {
                IsUnique = true
            });

            return this;
        }

        public StringFieldBuilder Pattern(string pattern, string? message = null)
        {
            Properties<StringFieldProperties>(p => p with
            {
                Pattern = pattern,
                PatternMessage = message
            });

            return this;
        }

        public StringFieldBuilder Length(int maxLength, int minLength = 0)
        {
            Properties<StringFieldProperties>(p => p with
            {
                MaxLength = maxLength,
                MinLength = minLength
            });

            return this;
        }
    }
}
