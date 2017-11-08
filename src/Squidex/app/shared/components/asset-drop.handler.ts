/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { AssetUrlPipe } from './pipes';
import { ApiUrlConfig, AssetDto } from './../declarations-base';

export class AssetDropHandler {

    private assetUrlGenerator: AssetUrlPipe;

    constructor(private readonly apiUrlConfig: ApiUrlConfig) {
        this.assetUrlGenerator = new AssetUrlPipe(this.apiUrlConfig);
    }

    public buildDroppedAssetData(asset: AssetDto, dragEvent: DragEvent) {
        if (asset.isImage) {
            return this.handleImageAsset(asset, dragEvent);
        }
        return '';
    }

    private handleImageAsset(asset: AssetDto, dragEvent: DragEvent) {
        let res = '<img src="' + this.assetUrlGenerator.transform(asset) + '" ';
        res += 'width="' + asset.pixelWidth + '" height="' + asset.pixelHeight + '">';
        return res;
    }
}