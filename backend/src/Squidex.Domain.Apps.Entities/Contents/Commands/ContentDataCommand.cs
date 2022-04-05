// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.Contents.Commands
{
    public abstract class ContentDataCommand : ContentCommand
    {
        public ContentData Data { get; set; }

        public bool DoNotValidate { get; set; }

        public bool DoNotValidateWorkflow { get; set; }

        public bool OptimizeValidation { get; set; }
    }
}
