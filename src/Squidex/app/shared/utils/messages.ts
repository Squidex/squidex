/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AssetDto } from './../services/assets.service';

export class HistoryChannelUpdated { }

export class AssetUpdated {
    constructor(
        public readonly assetDto: AssetDto,
        public readonly sender: any
    ) {
    }
}

export class AssetDragged {

    public static readonly DRAG_START = 'Start';
    public static readonly DRAG_END = 'End';

    constructor(
        public readonly assetDto: AssetDto,
        public readonly dragEvent: string,
        public readonly sender: any
    ) {
    }
}