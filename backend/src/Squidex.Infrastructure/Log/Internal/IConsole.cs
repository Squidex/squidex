// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Log.Internal
{
    public interface IConsole
    {
        void WriteLine(int color, string message);

        void Reset();
    }
}
