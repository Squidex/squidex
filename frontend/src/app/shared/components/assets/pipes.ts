/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';
import { Version } from '@app/framework';
import { ApiUrlConfig, AssetDto, AuthService, MathHelper, StringHelper, Types } from '@app/shared/internal';

@Pipe({
    name: 'sqxAssetUrl',
    pure: true,
    standalone: true,
})
export class AssetUrlPipe implements PipeTransform {
    constructor(
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public transform(asset: AssetDto, version?: number | Version, withQuery = false): string {
        const url = asset.fullUrl(this.apiUrl);

        const query: Record<string, any> = {};
        if (withQuery) {
            query['sq'] = MathHelper.guid();
        }

        if (Types.isNumber(version)) {
            query['version'] = version;
        } else if (Types.is(version, Version)) {
            query['version'] = version.value;
        }

        return url + StringHelper.buildQuery(query);
    }
}

@Pipe({
    name: 'sqxAssetPreviewUrl',
    pure: true,
    standalone: true,
})
export class AssetPreviewUrlPipe implements PipeTransform {
    constructor(
        private readonly apiUrl: ApiUrlConfig,
        private readonly authService: AuthService,
    ) {
    }

    public transform(asset: AssetDto): string {
        const url = asset.fullUrl(this.apiUrl);

        const query: Record<string, any> = {
            version: asset.version,
        };

        if (this.authService.user) {
            query['access_token'] = this.authService.user.accessToken;
        }

        return url + StringHelper.buildQuery(query);
    }
}

@Pipe({
    name: 'sqxFileIcon',
    pure: true,
    standalone: true,
})
export class FileIconPipe implements PipeTransform {
    public transform(asset: { mimeType: string; fileType: string }): string {
        let mimeIcon: string;

        const mimeParts = asset.mimeType.split('/');

        if (mimeParts.length === 2 && mimeParts[0].toLowerCase() === 'video') {
            mimeIcon = 'video';
        } else {
            mimeIcon = KNOWN_TYPES.includes(asset.fileType) ? asset.fileType : 'generic';
        }

        return `./images/asset_${mimeIcon}.svg`;
    }
}

@Pipe({
    name: 'sqxPreviewable',
    pure: true,
    standalone: true,
})
export class PreviewableType implements PipeTransform {
    public transform(asset: { fileSize: number; fileType: string }): boolean {
        return PREVIEW_TYPES.includes(asset.fileType) && asset.fileSize < 25_000_000;
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
    'xlsx',
];

const PREVIEW_TYPES: ReadonlyArray<string> = [
    'ai',
    'doc',
    'docx',
    'dxf',
    'eps',
    'pages',
    'pdf',
    'ppt',
    'pptx',
    'ps',
    'psd',
    'rar',
    'svg',
    'tiff',
    'ttf',
    'xls',
    'xlsx',
    'xps',
    'zip',
];
