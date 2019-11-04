﻿// ==========================================================================
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
    public class StringFieldBuilder : FieldBuilder
    {
        public StringFieldBuilder(UpsertSchemaField field, UpsertCommand schema)
            : base(field, schema)
        {
        }

        public StringFieldBuilder AsTextArea()
        {
            Properties<StringFieldProperties>().Editor = StringFieldEditor.TextArea;

            return this;
        }

        public StringFieldBuilder AsRichText()
        {
            Properties<StringFieldProperties>().Editor = StringFieldEditor.RichText;

            return this;
        }

        public StringFieldBuilder AsDropDown(params string[] values)
        {
            Properties<StringFieldProperties>().AllowedValues = ReadOnlyCollection.Create(values);
            Properties<StringFieldProperties>().Editor = StringFieldEditor.Dropdown;

            return this;
        }

        public StringFieldBuilder Pattern(string pattern, string? message = null)
        {
            Properties<StringFieldProperties>().Pattern = pattern;
            Properties<StringFieldProperties>().PatternMessage = message;

            return this;
        }

        public StringFieldBuilder Length(int maxLength, int minLength = 0)
        {
            Properties<StringFieldProperties>().MaxLength = maxLength;
            Properties<StringFieldProperties>().MinLength = minLength;

            return this;
        }
    }
}
