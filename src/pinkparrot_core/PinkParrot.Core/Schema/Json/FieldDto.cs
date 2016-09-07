// ==========================================================================
//  FieldDto.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

namespace PinkParrot.Core.Schema.Json
{
    public class FieldDto
    {
        public long Id { get; }

        public ModelFieldProperties Properties { get; }

        public FieldDto(long id, ModelFieldProperties properties)
        {
            Id = id;

            Properties = properties;
        }
    }
}
