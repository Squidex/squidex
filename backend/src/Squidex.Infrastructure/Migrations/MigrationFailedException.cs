// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;

namespace Squidex.Infrastructure.Migrations
{
    [Serializable]
    public class MigrationFailedException : Exception
    {
        public string Name { get; }

        public MigrationFailedException(string name, Exception? inner = null)
            : base(FormatException(name), inner)
        {
            Name = name;
        }

        protected MigrationFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Name = info.GetString(nameof(Name))!;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Name), Name);
        }

        private static string FormatException(string name)
        {
            return $"Failed to run migration '{name}'";
        }
    }
}
