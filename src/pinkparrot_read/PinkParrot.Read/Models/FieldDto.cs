// ==========================================================================
//  FieldDto.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json;
using PinkParrot.Core.Schemas;

namespace PinkParrot.Read.Models
{
    public class FieldDto
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public IFieldProperties Properties { get; set; }

        public static FieldDto Create(Field field)
        {
            return new FieldDto { Name = field.Name, Properties = field.RawProperties };
        }
    }
}