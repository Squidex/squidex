/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { interpolate } from '@app/framework';
import { AppSettingsDto } from './../services/apps.service';

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

    return interpolate(url, editors) || '';
}
