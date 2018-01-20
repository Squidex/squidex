// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Log
{
    public sealed class ConstantsLogWriter : ILogAppender
    {
        private readonly Action<IObjectWriter> objectWriter;

        public ConstantsLogWriter(Action<IObjectWriter> objectWriter)
        {
            Guard.NotNull(objectWriter, nameof(objectWriter));

            this.objectWriter = objectWriter;
        }

        public void Append(IObjectWriter writer)
        {
            objectWriter(writer);
        }
    }
}
