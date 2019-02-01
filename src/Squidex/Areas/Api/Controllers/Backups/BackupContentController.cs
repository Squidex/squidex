// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Commands;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.Backups
{
    /// <summary>
    /// Manages backups for app.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Backups))]
    public class BackupContentController : ApiController
    {
        private readonly IAssetStore assetStore;

        public BackupContentController(ICommandBus commandBus, IAssetStore assetStore)
            : base(commandBus)
        {
            this.assetStore = assetStore;
        }

        /// <summary>
        /// Get the backup content.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the asset.</param>
        /// <returns>
        /// 200 => Backup found and content returned.
        /// 404 => Backup or app not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/backups/{id}")]
        [ResponseCache(Duration = 3600 * 24 * 30)]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ApiCosts(0)]
        [AllowAnonymous]
        public IActionResult GetBackupContent(string app, Guid id)
        {
            return new FileCallbackResult("application/zip", "Backup.zip", false, bodyStream =>
            {
                return assetStore.DownloadAsync(id.ToString(), 0, null, bodyStream);
            });
        }
    }
}
