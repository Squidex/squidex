﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Immutable;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName(nameof(StringField))]
    public sealed class StringFieldProperties : FieldProperties
    {
        public ImmutableList<string> AllowedValues { get; set; }

        public int? MinLength { get; set; }

        public int? MaxLength { get; set; }

        public string DefaultValue { get; set; }

        public string Pattern { get; set; }

        public string PatternMessage { get; set; }

        public StringFieldEditor Editor { get; set; }

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override Field CreateField(long id, string name, Partitioning partitioning)
        {
            return new StringField(id, name, partitioning, this);
        }
    }
}
