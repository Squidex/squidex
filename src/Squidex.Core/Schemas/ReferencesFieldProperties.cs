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

namespace Squidex.Core.Schemas
{
    [TypeName("References")]
    public sealed class ReferencesFieldProperties : FieldProperties
    {
        private Guid schemaId;

        public Guid SchemaId
        {
            get { return schemaId; }
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
            yield break;
        }
    }
}
