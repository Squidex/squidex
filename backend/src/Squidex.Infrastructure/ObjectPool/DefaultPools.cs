// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace Squidex.Infrastructure.ObjectPool
{
    public static class DefaultPools
    {
        public static readonly ObjectPool<MemoryStream> MemoryStream =
            new DefaultObjectPool<MemoryStream>(new MemoryStreamPooledObjectPolicy());

        public static readonly ObjectPool<StringBuilder> StringBuilder =
            new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());
    }
}
