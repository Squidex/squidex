// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [Equals(DoNotAddEqualityOperators = true)]
    public sealed class SchemaProperties : NamedElementPropertiesBase
    {
        public ReadOnlyCollection<string>? Tags { get; set; }

        public string? ContentsSidebarUrl { get; set; }

        public string? ContentSidebarUrl { get; set; }

        public bool ValidateOnPublish { get; set; }
    }
}