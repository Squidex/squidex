// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using Newtonsoft.Json;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public static class BackupSerializer
    {
        private static readonly JsonSerializer JsonSerializer = JsonSerializer.CreateDefault();

        public static void SerializeAsJson<T>(this Stream stream, T value)
        {
            using (var writer = new StreamWriter(stream))
            {
                JsonSerializer.Serialize(writer, value);
            }
        }

        public static T DeserializeAsJson<T>(this Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return (T)JsonSerializer.Deserialize(reader, typeof(T));
            }
        }

        public static void DeserializeAsJson<T>(this Stream stream, T result)
        {
            using (var reader = new StreamReader(stream))
            {
                JsonSerializer.Populate(reader, result);
            }
        }
    }
}
