// ==========================================================================
//  FieldDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Schemas;

namespace Squidex.Store.MongoDb.Schemas.Models
{
    public class FieldModel
    {
        public string Name { get; set; }

        public bool IsHidden { get; set; }

        public bool IsDisabled { get; set; }
        
        public FieldProperties Properties { get; set; }

        public static FieldModel Create(Field field)
        {
            return new FieldModel
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