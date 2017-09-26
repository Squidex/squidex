// ==========================================================================
//  ReferencesFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
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

        protected override IEnumerable<ValidationError> ValidateCore()
        {
            if (MaxItems.HasValue && MinItems.HasValue && MinItems.Value >= MaxItems.Value)
            {
                yield return new ValidationError("Max items must be greater than min items", nameof(MinItems), nameof(MaxItems));
            }
        }
    }
}
