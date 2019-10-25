// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Log
{
    public interface ILogStore
    {
        Task ReadLogAsync(string key, DateTime from, DateTime to, Stream stream);
    }
}
