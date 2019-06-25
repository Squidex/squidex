// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public interface IEnrichedContentEntity : IContentEntity
    {
        bool CanUpdate { get; }

        string StatusColor { get; }

        StatusInfo[] Nexts { get; }
    }
}
