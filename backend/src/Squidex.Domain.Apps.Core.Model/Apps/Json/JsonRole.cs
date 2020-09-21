// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class JsonRole
    {
        [JsonProperty]
        public string[] Permissions { get; set; }

        [JsonProperty]
        public JsonObject Properties { get; set; }
    }
}
