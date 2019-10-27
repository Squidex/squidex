// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Log
{
    public sealed class NoopLogStore : ILogStore
    {
        private static readonly byte[] NoopText = Encoding.UTF8.GetBytes("Not Supported");

        public Task ReadLogAsync(string key, DateTime from, DateTime to, Stream stream)
        {
            return stream.WriteAsync(NoopText, 0, NoopText.Length);
        }
    }
}
