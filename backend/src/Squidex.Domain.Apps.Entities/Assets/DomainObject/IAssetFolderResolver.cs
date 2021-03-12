﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject
{
    public interface IAssetFolderResolver
    {
        Task<DomainId> ResolveOrCreateAsync(Context context, ICommandBus commandBus, string path);
    }
}