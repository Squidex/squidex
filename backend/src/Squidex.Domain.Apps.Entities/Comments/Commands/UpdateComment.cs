﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Comments.Commands
{
    public sealed class UpdateComment : CommentsCommand
    {
        public string Text { get; set; }
    }
}
