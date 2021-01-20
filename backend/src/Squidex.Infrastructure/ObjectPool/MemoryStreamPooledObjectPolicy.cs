// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using Microsoft.Extensions.ObjectPool;

namespace Squidex.Infrastructure.ObjectPool
{
    public sealed class MemoryStreamPooledObjectPolicy : PooledObjectPolicy<MemoryStream>
    {
        public int InitialCapacity { get; set; } = 100;

        public int MaximumRetainedCapacity { get; set; } = 4 * 1024;

        public override MemoryStream Create()
        {
            return new MemoryStream(InitialCapacity);
        }

        public override bool Return(MemoryStream obj)
        {
            if (obj.Capacity > MaximumRetainedCapacity)
            {
                return false;
            }

            obj.Position = 0;

            return true;
        }
    }
}