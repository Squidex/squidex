// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName("ArrayField")]
    public sealed class ArrayFieldProperties : FieldProperties
    {
        public int? MinItems { get; set; }

        public int? MaxItems { get; set; }

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            throw new NotImplementedException();
        }

        public override T Accept<T>(IFieldVisitor<T> visitor, IField field)
        {
            throw new NotImplementedException();
        }

        public override Field CreateField(long id, string name, Partitioning partitioning)
        {
            throw new NotImplementedException();
        }
    }
}
