// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public interface IUserMapping
    {
        RefToken Initiator { get; }

        void Backup(RefToken token);

        void Backup(string userId);

        bool TryMap(RefToken token, out RefToken result);

        bool TryMap(string userId, out RefToken result);
    }
}