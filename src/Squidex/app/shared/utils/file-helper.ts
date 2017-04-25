/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ApiUrlConfig, MathHelper } from 'framework';
import { AssetDto } from './../services/assets.service';

const mimeMapping = {
    'pdf': 'pdf',
    'vnd.openxmlformats-officedocument.wordprocessingml.document': 'docx',
    'vnd.openxmlformats-officedocument.wordprocessingml.template': 'docx',
    'vnd.openxmlformats-officedocument.spreadsheetml.sheet': 'xlsx',
    'vnd.openxmlformats-officedocument.spreadsheetml.template': 'xlsx',
    'vnd.openxmlformats-officedocument.presentationml.presentation': 'pptx',
    'vnd.openxmlformats-officedocument.presentationml.template': 'pptx',
    'vnd.openxmlformats-officedocument.presentationml.slideshow': 'pptx',
    'msword': 'doc',
    'vnd.ms-word': 'doc',
    'vnd.ms-word.document.macroEnabled.12': 'docx',
    'vnd.ms-word.template.macroEnabled.12': 'docx',
    'vnd.ms-excel': 'xls',
    'vnd.ms-excel.sheet.macroEnabled.12': 'xlsx',
    'vnd.ms-excel.template.macroEnabled.12': 'xlsx',
    'vnd.ms-excel.addin.macroEnabled.12': 'xlsx',
    'vnd.ms-excel.sheet.binary.macroEnabled.12': 'xlsx',
    'vnd.ms-powerpoint': 'ppt',
    'vnd.ms-powerpoint.addin.macroEnabled.12': 'pptx',
    'vnd.ms-powerpoint.presentation.macroEnabled.12': 'pptx',
    'vnd.ms-powerpoint.template.macroEnabled.12': 'pptx',
    'vnd.ms-powerpoint.slideshow.macroEnabled.12': 'pptx'
};

export module FileHelper {
    export function assetUrl(apiUrl: ApiUrlConfig, asset: AssetDto): string {
        return apiUrl.buildUrl(`api/assets/${asset.id}?q=${MathHelper.guid()}`);
    }

    export function assetName(asset: AssetDto): string {
        return asset.fileName;
    }

    export function assetPreviewUrl(apiUrl: ApiUrlConfig, asset: AssetDto) {
        return apiUrl.buildUrl(`api/assets/${asset.id}?version=${asset.version.value}`);
    }

    export function assetInfo(asset: AssetDto): string {
        let result = '';

        if (asset != null) {
            if (asset.pixelWidth) {
                result = `${asset.pixelWidth}x${asset.pixelHeight}px, `;
            }

            result += FileHelper.fileSize(asset.fileSize);
        }

        return result;
    }

    export function fileType(mimeType: string, fileName: string) {
        if (fileName) {
            const parts = fileName.split('.');

            if (parts.length > 1) {
                return parts[parts.length - 1].toLowerCase();
            }
        }
        if (mimeType) {
            const parts = mimeType.split('/');

            if (parts.length === 2) {
                const mimeSuffix = parts[1].toLowerCase();

                return mimeMapping[mimeSuffix] || mimeSuffix;
            }
        }
        return undefined;
    }

    export function fileIcon(mimeType: string) {
        const mimeParts = mimeType.split('/');

        let mimeIcon = 'generic';

        if (mimeParts.length === 2) {
            const mimePrefix = mimeParts[0].toLowerCase();
            const mimeSuffix = mimeParts[1].toLowerCase();

            if (mimePrefix === 'video') {
                mimeIcon = 'video';
            } else {
                mimeIcon = mimeMapping[mimeSuffix] || 'generic';
            }
        }

        return `/images/asset_${mimeIcon}.png`;
    }

    export function fileSize(bytes: number) {
        let u = 0, s = 1024;

        while (bytes >= s || -bytes >= s) {
            bytes /= s;
            u++;
        }

        return (u ? bytes.toFixed(1) + ' ' : bytes) + ' kMGTPEZY'[u] + 'B';
    }
}