// ==========================================================================
//  StringFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName(nameof(StringField))]
    public sealed class StringFieldProperties : FieldProperties
    {
        public int? MinLength { get; set; }

        public int? MaxLength { get; set; }

        public string DefaultValue { get; set; }

        public string Pattern { get; set; }

        public string PatternMessage { get; set; }

        public string[] AllowedValues { get; set; }

        public StringFieldEditor Editor { get; set; }

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
