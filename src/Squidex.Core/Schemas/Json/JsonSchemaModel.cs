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
        public string Name;

        public bool IsPublished;

        public SchemaProperties Properties;

        public Dictionary<long, JsonFieldModel> Fields;
    }
}