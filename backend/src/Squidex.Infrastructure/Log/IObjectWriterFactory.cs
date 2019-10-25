// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Log
{
    public interface IObjectWriterFactory
    {
        IObjectWriter Create();

        void Release(IObjectWriter writer);
    }
}
