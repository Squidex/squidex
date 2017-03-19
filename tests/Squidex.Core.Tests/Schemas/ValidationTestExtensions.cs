// ==========================================================================
//  ValidationTestExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Squidex.Core.Schemas
{
    public static class ValidationTestExtensions
    {
        public static Task ValidateAsync(this Field field, JToken value, IList<string> errors)
        {
            return field.ValidateAsync(value, errors.Add);
        }
    }
}
