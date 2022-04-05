// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas
{
    public abstract record NamedElementPropertiesBase
    {
        public string? Label { get; init; }

        public string? Hints { get; init; }
    }
}
