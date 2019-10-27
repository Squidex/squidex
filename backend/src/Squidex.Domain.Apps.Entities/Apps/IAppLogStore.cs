// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public interface IAppLogStore
    {
        Task ReadLogAsync(string appId, DateTime from, DateTime to, Stream stream);
    }
}
