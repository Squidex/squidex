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
    [TypeName(nameof(GeolocationField))]
    public sealed class GeolocationFieldProperties : FieldProperties
    {
        private GeolocationFieldEditor editor;

        public GeolocationFieldEditor Editor
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
            return null;
        }

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
