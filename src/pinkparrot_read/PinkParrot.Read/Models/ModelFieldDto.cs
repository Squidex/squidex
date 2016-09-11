// ==========================================================================
//  ModelFieldDto.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json;
using PinkParrot.Core.Schema;

namespace PinkParrot.Read.Models
{
    public class ModelFieldDto
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public IModelFieldProperties Properties { get; set; }

        public static ModelFieldDto Create(ModelField field)
        {
            return new ModelFieldDto { Name = field.Name, Properties = field.RawProperties };
        }
    }
}