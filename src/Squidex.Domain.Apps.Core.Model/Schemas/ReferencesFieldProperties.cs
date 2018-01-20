﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override Field CreateField(long id, string name, Partitioning partitioning)
        {
            return new ReferencesField(id, name, partitioning, this);
        }
    }
}
