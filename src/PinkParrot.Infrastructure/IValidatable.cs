// ==========================================================================
//  IValidatable.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;

namespace PinkParrot.Infrastructure
{
    public interface IValidatable
    {
        void Validate(IList<ValidationError> errors);
    }
}
