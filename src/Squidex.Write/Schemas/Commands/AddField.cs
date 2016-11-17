// ==========================================================================
//  AddField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;

namespace Squidex.Write.Schemas.Commands
{
    public class AddField : AppCommand, IValidatable
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public JToken Properties { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (!Name.IsSlug())
            {
                errors.Add(new ValidationError("DisplayName must be a valid slug", nameof(Name)));
            }

            if (string.IsNullOrWhiteSpace(Type))
            {
                errors.Add(new ValidationError("Type must be specified", nameof(Type)));
            }

            if (Properties != null && !(Properties is JObject))
            {
                errors.Add(new ValidationError("Properties must be a object or null.", nameof(Properties)));
            }
        }
    }
}