// ==========================================================================
//  ReferencesFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName(nameof(ReferencesField))]
    public sealed class ReferencesFieldProperties : FieldProperties
    {
        private int? minItems;
        private int? maxItems;
        private Guid schemaId;

        public int? MinItems
        {
            get
            {
                return minItems;
            }
            set
            {
                ThrowIfFrozen();

                minItems = value;
            }
        }

        public int? MaxItems
        {
            get
            {
                return maxItems;
            }
            set
            {
                ThrowIfFrozen();

                maxItems = value;
            }
        }

        public Guid SchemaId
        {
            get
            {
                return schemaId;
            }
            set
            {
                ThrowIfFrozen();

                schemaId = value;
            }
        }

        public override JToken GetDefaultValue()
        {
            return new JArray();
        }

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
