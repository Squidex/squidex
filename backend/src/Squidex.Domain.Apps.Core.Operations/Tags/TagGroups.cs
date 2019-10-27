// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Core.Tags
{
    public static class TagGroups
    {
        public const string Assets = "Assets";

        public static string Schemas(Guid schemaId)
        {
            return $"Schemas_{schemaId}";
        }
    }
}
