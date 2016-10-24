// ==========================================================================
//  UpdateField.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PinkParrot.Infrastructure;

namespace PinkParrot.Write.Schemas.Commands
{
    public class UpdateField : AppCommand, IValidatable
    {
        public long FieldId { get; set; }

        public JToken Properties { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (!(Properties is JObject))
            {
                errors.Add(new ValidationError("Properties must be a object.", nameof(Properties)));
            }
        }
    }
}