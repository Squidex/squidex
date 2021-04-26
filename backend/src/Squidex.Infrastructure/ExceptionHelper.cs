// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure
{
    public static class ExceptionHelper
    {
        public static bool Is<T>(this Exception ex) where T : Exception
        {
            if (ex is AggregateException aggregateException)
            {
                aggregateException = aggregateException.Flatten();

                return aggregateException.InnerExceptions.Count == 1 && Is<T>(aggregateException.InnerExceptions[0]);
            }

            return ex is T;
        }
    }
}
