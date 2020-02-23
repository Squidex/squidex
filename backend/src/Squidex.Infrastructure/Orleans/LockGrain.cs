// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class LockGrain : GrainOfString, ILockGrain
    {
        private readonly Dictionary<string, string> locks = new Dictionary<string, string>();

        public Task<string?> AcquireLockAsync(string key)
        {
            string? releaseToken = null;

            if (!locks.ContainsKey(key))
            {
                releaseToken = Guid.NewGuid().ToString();

                locks.Add(key, releaseToken);
            }

            return Task.FromResult(releaseToken);
        }

        public Task ReleaseLockAsync(string releaseToken)
        {
            var key = locks.FirstOrDefault(x => x.Value == releaseToken).Key;

            if (!string.IsNullOrWhiteSpace(key))
            {
                locks.Remove(key);
            }

            return Task.CompletedTask;
        }
    }
}
