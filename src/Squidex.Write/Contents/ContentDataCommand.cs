// ==========================================================================
//  ContentDataCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;

namespace Squidex.Write.Contents
{
    public class ContentDataCommand : SchemaCommand, IValidatable
    {
        public JObject Data { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (Data == null)
            {
                errors.Add(new ValidationError("Data cannot be null", nameof(Data)));
            }
        }
    }
}
