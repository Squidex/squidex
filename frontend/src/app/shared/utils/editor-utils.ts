/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { interpolate } from '@app/framework';
import { AppSettingsDto } from '../model';

export function computeEditorUrl(url?: string | null, settings?: AppSettingsDto | null) {
    if (!url) {
        return '';
    }

    const editors: { [key: string]: string } = {};
    if (settings?.editors) {
        for (const editor of settings.editors) {
            editors[editor.name] = editor.url;
        }
    }

    const result = interpolate(url, editors) || '';

    return isSafeEditorUrl(result) ? result : '';
}

function isSafeEditorUrl(url: string) {
    if (!url) {
        return false;
    }

    // The url is trusted for the iframe, therefore protocols like 'javascript:' would run in our origin.
    try {
        const protocol = new URL(url, window.location.href).protocol;

        return protocol === 'http:' || protocol === 'https:';
    } catch {
        return false;
    }
}
