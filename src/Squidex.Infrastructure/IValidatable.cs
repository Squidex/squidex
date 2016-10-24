// ==========================================================================
//  IValidatable.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Infrastructure
{
    public interface IValidatable
    {
        void Validate(IList<ValidationError> errors);
    }
}
