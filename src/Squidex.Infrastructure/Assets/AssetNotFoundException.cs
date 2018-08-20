// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;

namespace Squidex.Infrastructure.Assets
{
    [Serializable]
    public class AssetNotFoundException : Exception
    {
        public AssetNotFoundException(string fileName)
            : base(FormatMessage(fileName))
        {
        }

        public AssetNotFoundException(string fileName, Exception inner)
            : base(FormatMessage(fileName), inner)
        {
        }

        protected AssetNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private static string FormatMessage(string fileName)
        {
            Guard.NotNullOrEmpty(fileName, nameof(fileName));

            return $"An asset with name '{fileName}' does not exist.";
        }
    }
}
