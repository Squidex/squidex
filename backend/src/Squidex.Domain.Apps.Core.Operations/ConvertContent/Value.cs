// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public static class Value
    {
        public static readonly IJsonValue Unset = JsonValue.Create("UNSET");
    }
}
