﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Entities.Comments.Commands
{
    public abstract class CommentTextCommand : CommentsCommand
    {
        public string Text { get; set; }

        public string[]? Mentions { get; set; }
    }
}
