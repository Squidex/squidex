// ==========================================================================
//  IValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Core.Schemas
{
    public interface IValidator
    {
        Task ValidateAsync(object value, ICollection<string> errors);
    }
}
