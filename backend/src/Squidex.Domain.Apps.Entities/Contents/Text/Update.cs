// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class Update
    {
        public Guid Id { get; set; }

        public NamedContentData Data { get; set; }

        public bool OnlyDraft { get; set; }
    }
}
