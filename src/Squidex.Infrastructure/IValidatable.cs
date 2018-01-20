// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Infrastructure
{
    public interface IValidatable
    {
        void Validate(IList<ValidationError> errors);
    }
}
