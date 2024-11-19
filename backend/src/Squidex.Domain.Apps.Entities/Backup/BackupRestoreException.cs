// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Backup;

[Serializable]
public class BackupRestoreException(string message, Exception? inner = null) : Exception(message, inner)
{
}
