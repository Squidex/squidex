// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;

namespace Squidex.Domain.Apps.Core.Schemas.Json
{
    public sealed class JsonNestedFieldModel : IFieldSettings
    {
        [JsonProperty]
        public long Id { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public bool IsHidden { get; set; }

        [JsonProperty]
        public bool IsLocked { get; set; }

        [JsonProperty]
        public bool IsDisabled { get; set; }

        [JsonProperty]
        public FieldProperties Properties { get; set; }

        public NestedField ToNestedField()
        {
            return Properties.CreateNestedField(Id, Name, this);
        }
    }
}
