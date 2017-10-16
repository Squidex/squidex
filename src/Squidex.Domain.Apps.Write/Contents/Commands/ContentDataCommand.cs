// ==========================================================================
//  ContentDataCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Contents.Commands
{
    public abstract class ContentDataCommand : ContentCommand, IValidatable
    {
        public NamedContentData Data { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (Data == null)
            {
                errors.Add(new ValidationError("Data cannot be null.", nameof(Data)));
            }
        }
    }
}
