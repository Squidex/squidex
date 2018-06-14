// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Squidex.Domain.Apps.Core.Schemas.Json
{
    public sealed class JsonFieldModel
    {
        [JsonProperty]
        public long Id { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string Partitioning { get; set; }

        [JsonProperty]
        public bool IsHidden { get; set; }

        [JsonProperty]
        public bool IsLocked { get; set; }

        [JsonProperty]
        public bool IsDisabled { get; set; }

        [JsonProperty]
        public FieldProperties Properties { get; set; }

        [JsonProperty]
        public List<JsonNestedFieldModel> Children { get; set; }
    }
}