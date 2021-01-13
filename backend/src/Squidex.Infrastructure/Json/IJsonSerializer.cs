// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;

namespace Squidex.Infrastructure.Json
{
    public interface IJsonSerializer
    {
        string Serialize<T>(T value, bool intented = false);

        void Serialize<T>(T value, Stream stream, bool leaveOpen = false);

        T Deserialize<T>(string value, Type? actualType = null);

        T Deserialize<T>(Stream stream, Type? actualType = null, bool leaveOpen = false);
    }
}
