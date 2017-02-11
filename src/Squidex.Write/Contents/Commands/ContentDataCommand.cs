// ==========================================================================
//  ContentDataCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Write.Contents.Commands
{
    public abstract class ContentDataCommand : ContentCommand, IValidatable
    {
        public ContentData Data { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (Data == null)
            {
                errors.Add(new ValidationError("Data cannot be null", nameof(Data)));
            }
        }
    }
}
