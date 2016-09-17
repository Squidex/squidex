// ==========================================================================
//  FieldDto.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Core.Schemas;

namespace PinkParrot.Read.Models
{
    public class FieldDto
    {
        public string Name { get; set; }

        public bool IsHidden { get; set; }

        public bool IsDisabled { get; set; }
        
        public FieldProperties Properties { get; set; }

        public static FieldDto Create(Field field)
        {
            return new FieldDto
            {
                Name = field.Name,
                IsHidden = field.IsHidden,
                IsDisabled = field.IsDisabled,
                Properties = field.RawProperties
            };
        }

        public Field ToField(long id, FieldRegistry registry)
        {
            var field = registry.CreateField(id, Name, Properties);

            if (IsHidden)
            {
                field = field.Hide();
            }

            if (IsDisabled)
            {
                field = field.Disable();
            }

            return field;
        }
    }
}