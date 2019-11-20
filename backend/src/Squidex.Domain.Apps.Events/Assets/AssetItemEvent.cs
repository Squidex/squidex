// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Events.Assets
{
    public abstract class AssetItemEvent : AppEvent
    {
        public Guid AssetItemId { get; set; }

        public Guid AssetId
        {
            get
            {
                return AssetId;
            }
            set
            {
                AssetId = value;
            }
        }

        public Guid AssetFolderId
        {
            get
            {
                return AssetId;
            }
            set
            {
                AssetId = value;
            }
        }
    }
}
