/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';

import {
    ApiUrlConfig,
    AssetDto,
    MathHelper
} from '@app/shared/internal';

@Pipe({
    name: 'sqxAssetUrl',
    pure: true
})
export class AssetUrlPipe implements PipeTransform {
    constructor(
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public transform(asset: AssetDto): string {
        return `${asset.fullUrl(this.apiUrl)}&sq=${MathHelper.guid()}`;
    }
}

@Pipe({
    name: 'sqxAssetPreviewUrl',
    pure: true
})
export class AssetPreviewUrlPipe implements PipeTransform {
    constructor(
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public transform(asset: AssetDto): string {
        return asset.fullUrl(this.apiUrl);
    }
}

@Pipe({
    name: 'sqxFileIcon',
    pure: true
})
export class FileIconPipe implements PipeTransform {
    public transform(asset: { mimeType: string, fileType: string }): string {
        const knownTypes = [
            'doc',
            'docx',
            'pdf',
            'ppt',
            'pptx',
            'video',
            'xls',
            'xlsx'
        ];

        let mimeIcon: string;

        const mimeParts = asset.mimeType.split('/');

        if (mimeParts.length === 2 && mimeParts[0].toLowerCase() === 'video') {
            mimeIcon = 'video';
        } else {
            mimeIcon = knownTypes.indexOf(asset.fileType) >= 0 ? asset.fileType : 'generic';
        }

        return `./images/asset_${mimeIcon}.svg`;
    }
}