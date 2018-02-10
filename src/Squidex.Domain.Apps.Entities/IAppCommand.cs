// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities
{
    public interface IAppCommand : ICommand
    {
        NamedId<Guid> AppId { get; set; }
    }
}
