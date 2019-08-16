// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents.Commands
{
    public abstract class ContentUpdateCommand : ContentDataCommand
    {
        public bool AsDraft { get; set; }
    }
}
