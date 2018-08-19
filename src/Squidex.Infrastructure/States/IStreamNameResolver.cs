// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.States
{
    public interface IStreamNameResolver
    {
        string GetStreamName(Type aggregateType, string id);

        string WithNewId(string streamName, Func<string, string> idGenerator);
    }
}
