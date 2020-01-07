// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;
using DeepEqual.Syntax;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class SchemaProperties : NamedElementPropertiesBase
    {
        public ReadOnlyCollection<string> Tags { get; set; }

        public bool DeepEquals(SchemaProperties properties)
        {
            return this.WithDeepEqual(properties).IgnoreProperty<Freezable>(x => x.IsFrozen).Compare();
        }
    }
}