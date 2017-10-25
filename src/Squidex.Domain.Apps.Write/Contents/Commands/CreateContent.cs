// ==========================================================================
//  CreateContent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Domain.Apps.Write.Contents.Commands
{
    public sealed class CreateContent : ContentDataCommand
    {
        public bool Publish { get; set; }
    }
}
