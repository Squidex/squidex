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

namespace Squidex.Domain.Apps.Entities.Schemas.State
{
    public sealed class SchemaState : DomainObjectState<SchemaState>,
        ISchemaEntity,
        IUpdateableEntityWithAppRef,
        IUpdateableEntityWithCreatedBy,
        IUpdateableEntityWithLastModifiedBy
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public Guid AppId { get; set; }

        [JsonProperty]
        public int TotalFields { get; set; }

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

        [JsonIgnore]
        public bool IsPublished
        {
            get { return SchemaDef.IsPublished; }
        }
    }
}
