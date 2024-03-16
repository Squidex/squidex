// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Microsoft.Extensions.ObjectPool;
using Microsoft.IO;

namespace Squidex.Infrastructure.ObjectPool
{
    public static class DefaultPools
    {
        public static readonly RecyclableMemoryStreamManager MemoryStream =
            new RecyclableMemoryStreamManager();

        public static readonly ObjectPool<StringBuilder> StringBuilder =
            new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());
    }
}
