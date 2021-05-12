// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public interface IMetadataProvider
    {
        IDictionary<string, object> Metadata { get; }

        T? GetMetadata<T>(string key, T? defaultValue = default);

        T GetMetadata<T>(string key, Func<T> defaultValueFactory);

        bool HasMetadata(string key);
    }
}
