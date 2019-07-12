// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Infrastructure;
using P = Squidex.Domain.Apps.Core.Partitioning;

namespace Squidex.Domain.Apps.Core.Schemas.Json
{
    public sealed class JsonFieldModel : IFieldSettings
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
        public JsonNestedFieldModel[] Children { get; set; }

        public RootField ToField()
        {
            var partitioning = P.FromString(Partitioning);

            if (Properties is ArrayFieldProperties arrayProperties)
            {
                var nested = Children?.Map(n => n.ToNestedField()) ?? Array.Empty<NestedField>();

                return new ArrayField(Id, Name, partitioning, nested, arrayProperties, this);
            }
            else
            {
                return Properties.CreateRootField(Id, Name, partitioning, this);
            }
        }
    }
}