/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { LocalStoreService, Version } from '@app/framework';

export declare type AutoSaveKey = { schemaId: string; schemaVersion: Version; contentId?: string };

@Injectable()
export class AutoSaveService {
    constructor(
        private readonly localStore: LocalStoreService,
    ) {
    }

    public fetch(key: AutoSaveKey): {} | null {
        if (!key) {
            return null;
        }

        const value = this.localStore.get(getKey(key));

        if (value) {
            this.remove(key);

            return JSON.parse(value);
        }

        return null;
    }

    public set(key: AutoSaveKey, content: {}) {
        if (!key || !content) {
            return;
        }

        const json = JSON.stringify(content);

        this.localStore.set(getKey(key), json);
    }

    public remove(key: AutoSaveKey) {
        if (!key) {
            return;
        }

        this.localStore.remove(getKey(key));
    }
}

function getKey(key: AutoSaveKey) {
    const { schemaId, schemaVersion } = key;

    let contentId = key.contentId;

    if (!contentId) {
        contentId = '';
    } else {
        contentId = `.${contentId}`;
    }

    return `autosave.${schemaId}-${schemaVersion.value}${contentId}`;
}
