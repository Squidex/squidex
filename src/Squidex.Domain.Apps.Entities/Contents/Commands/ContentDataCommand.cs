// ==========================================================================
//  ContentDataCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.Contents.Commands
{
    public abstract class ContentDataCommand : ContentCommand
    {
        public NamedContentData Data { get; set; }
    }
}
