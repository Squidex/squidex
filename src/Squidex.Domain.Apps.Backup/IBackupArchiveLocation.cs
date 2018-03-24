using System;
using System.IO;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Backup
{
    public interface IBackupArchiveLocation
    {
        Task<Stream> OpenStreamAsync(Guid backupId);

        Task DeleteArchiveAsync(Guid backupId);
    }
}
