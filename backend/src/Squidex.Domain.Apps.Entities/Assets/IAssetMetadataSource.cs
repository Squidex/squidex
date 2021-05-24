// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets.Commands;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public interface IAssetMetadataSource
    {
        int Order => 0;

        Task EnhanceAsync(UploadAssetCommand command);

        IEnumerable<string> Format(IAssetEntity asset);
    }
}
