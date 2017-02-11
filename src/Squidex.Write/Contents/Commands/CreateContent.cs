// ==========================================================================
//  CreateContent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Write.Contents.Commands
{
    public class CreateContent : ContentDataCommand
    {
        public CreateContent()
        {
            ContentId = Guid.NewGuid();
        }
    }
}
