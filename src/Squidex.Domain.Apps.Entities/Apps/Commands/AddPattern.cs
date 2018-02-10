// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class AddPattern : AppCommand
    {
        public Guid PatternId { get; set; }

        public string Name { get; set; }

        public string Pattern { get; set; }

        public string Message { get; set; }

        public AddPattern()
        {
            PatternId = Guid.NewGuid();
        }
    }
}
