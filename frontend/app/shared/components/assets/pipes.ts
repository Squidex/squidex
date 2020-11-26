/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';
import { ApiUrlConfig, AssetDto, AuthService, MathHelper, StringHelper, Types } from '@app/shared/internal';

@Pipe({
    name: 'sqxAssetUrl',
    pure: true
})
export class AssetUrlPipe implements PipeTransform {
    constructor(
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public transform(asset: AssetDto, version?: number, withQuery = false): string {
        let url = asset.fullUrl(this.apiUrl);

        if (withQuery) {
            url = StringHelper.appendToUrl(url, 'sq', MathHelper.guid());
        }

        if (Types.isNumber(version)) {
            url = StringHelper.appendToUrl(url, 'version', version);
        }

        return url;
    }
}

@Pipe({
    name: 'sqxAssetPreviewUrl',
    pure: true
})
export class AssetPreviewUrlPipe implements PipeTransform {
    constructor(
        private readonly apiUrl: ApiUrlConfig,
        private readonly authService: AuthService
    ) {
    }

    public transform(asset: AssetDto): string {
        let url =  asset.fullUrl(this.apiUrl, this.authService);

        url = StringHelper.appendToUrl(url, 'version', asset.fileVersion);

        return url;
    }
}

@Pipe({
    name: 'sqxFileIcon',
    pure: true
})
export class FileIconPipe implements PipeTransform {
    public transform(asset: { mimeType: string, fileType: string }): string {

        let mimeIcon: string;

        const mimeParts = asset.mimeType.split('/');

        if (mimeParts.length === 2 && mimeParts[0].toLowerCase() === 'video') {
            mimeIcon = 'video';
        } else {
            mimeIcon = KNOWN_TYPES.indexOf(asset.fileType) >= 0 ? asset.fileType : 'generic';
        }

        return `./images/asset_${mimeIcon}.svg`;
    }
}

const KNOWN_TYPES: ReadonlyArray<string> = [
    'doc',
    'docx',
    'pdf',
    'ppt',
    'pptx',
    'video',
    'xls',
    'xlsx'
];