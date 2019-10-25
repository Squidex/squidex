// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;

namespace Squidex.Domain.Apps.Entities.Backup
{
    [Serializable]
    public class BackupRestoreException : Exception
    {
        public BackupRestoreException(string message)
            : base(message)
        {
        }

        public BackupRestoreException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected BackupRestoreException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
