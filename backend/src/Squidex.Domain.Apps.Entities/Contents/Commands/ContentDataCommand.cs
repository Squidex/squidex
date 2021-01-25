// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.Contents.Commands
{
    public abstract class ContentDataCommand : ContentCommand
    {
        public bool DoNotValidate { get; set; }

        public bool OptimizeValidation { get; set; }

        public ContentData Data { get; set; }
    }
}
