// ==========================================================================
//  JsonSchemaModel.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Core.Schemas.Json
{
    public sealed class JsonSchemaModel
    {
        public string Name { get; set; }

        public bool IsPublished { get; set; }

        public SchemaProperties Properties { get; set; }

        public List<JsonFieldModel> Fields { get; set; }
    }
}