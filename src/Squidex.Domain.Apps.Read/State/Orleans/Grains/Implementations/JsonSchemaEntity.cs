// ==========================================================================
//  JsonSchemaEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains.Implementations
{
    public sealed class JsonSchemaEntity : JsonEntity, ISchemaEntity
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public Guid AppId { get; set; }

        [JsonProperty]
        public RefToken CreatedBy { get; set; }

        [JsonProperty]
        public RefToken LastModifiedBy { get; set; }

        [JsonProperty]
        public bool IsDeleted { get; set; }

        [JsonProperty]
        public string ScriptQuery { get; set; }

        [JsonProperty]
        public string ScriptCreate { get; set; }

        [JsonProperty]
        public string ScriptUpdate { get; set; }

        [JsonProperty]
        public string ScriptDelete { get; set; }

        [JsonProperty]
        public string ScriptChange { get; set; }

        [JsonProperty]
        public Schema SchemaDef { get; set; }

        public bool IsPublished
        {
            get { return SchemaDef.IsPublished; }
        }
    }
}
