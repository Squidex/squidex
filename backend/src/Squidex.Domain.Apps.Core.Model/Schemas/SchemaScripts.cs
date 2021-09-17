// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed record SchemaScripts
    {
        public static readonly SchemaScripts Empty = new SchemaScripts();

        public string? Change { get; init; }

        public string? Create { get; init; }

        public string? Update { get; init; }

        public string? Delete { get; init; }

        public string? Query { get; init; }
    }
}
