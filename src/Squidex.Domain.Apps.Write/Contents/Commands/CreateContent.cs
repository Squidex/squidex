// ==========================================================================
//  CreateContent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Write.Contents.Commands
{
    public class CreateContent : ContentDataCommand
    {
        public bool Publish { get; set; }

        public CreateContent()
        {
            ContentId = Guid.NewGuid();
        }
    }
}
