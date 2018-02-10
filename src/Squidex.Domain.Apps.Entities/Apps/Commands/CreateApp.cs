// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class CreateApp : AppCommand, IAggregateCommand
    {
        public string Name { get; set; }

        public string Template { get; set; }

        public CreateApp()
        {
            AppId = Guid.NewGuid();
        }
    }
}