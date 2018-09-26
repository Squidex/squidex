// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public interface IContentResolver
    {
        Task<NamedContentData> GetContentDataAsync(Guid id);
    }
}
