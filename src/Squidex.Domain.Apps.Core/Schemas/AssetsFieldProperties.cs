// ==========================================================================
//  AssetsFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName("AssetsField")]
    public sealed class AssetsFieldProperties : FieldProperties
    {
        private int? minItems;
        private int? maxItems;

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
