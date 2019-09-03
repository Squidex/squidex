/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';

import { LocalStoreService, Version } from '@app/framework';

export declare type AutoSaveKey = { schemaId: string, schemaVersion: Version, contentId?: string };

@Injectable()
export class AutoSaveService {
    constructor(
        private readonly localStore: LocalStoreService
    ) {
    }

    public get(key: AutoSaveKey): object | null {
        if (!key) {
            return null;
        }

        const value = this.localStore.get(getKey(key));

        if (value) {
            return JSON.parse(value);
        }

        return null;
    }

    public set(key: AutoSaveKey, content: object) {
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
    let { contentId, schemaId, schemaVersion } = key;

    if (!contentId) {
        contentId = '';
    } else {
        contentId = `.${contentId}`;
    }

    return `autosave.${schemaId}-${schemaVersion.value}${contentId}`;
}