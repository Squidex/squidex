// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

#pragma warning disable SYSLIB0011 // Type or member is obsolete

namespace Squidex.Infrastructure.TestHelpers
{
    public static class BinaryFormatterHelper
    {
        private static readonly BinaryFormatter Formatter = new BinaryFormatter();

        public static T SerializeAndDeserializeBinary<T>(this T source)
        {
            var stream = new MemoryStream();

            Formatter.Serialize(stream, source!);

            stream.Position = 0;

            return (T)Formatter.Deserialize(stream);
        }
    }
}
