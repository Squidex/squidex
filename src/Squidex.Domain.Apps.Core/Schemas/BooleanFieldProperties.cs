// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName(nameof(BooleanField))]
    public sealed class BooleanFieldProperties : FieldProperties
    {
        private BooleanFieldEditor editor;
        private bool? defaultValue;

        public bool? DefaultValue
        {
            get
            {
                return defaultValue;
            }
            set
            {
                ThrowIfFrozen();

                defaultValue = value;
            }
        }

        public BooleanFieldEditor Editor
        {
            get
            {
                return editor;
            }
            set
            {
                ThrowIfFrozen();

                editor = value;
            }
        }

        public override JToken GetDefaultValue()
        {
            return DefaultValue;
        }

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
